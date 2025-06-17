using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using DG.Tweening;
using Building.UI; // Добавляем using для UI namespace

namespace Building
{
    /// <summary>
    /// Основная система строительства
    /// </summary>
    public class BuildingSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Tilemap mainTilemap;
        [SerializeField] private Tilemap buildingTilemap; // Отдельный tilemap для зданий
        [SerializeField] private Transform buildingsContainer; // Контейнер для GameObject зданий
        [SerializeField] private Camera mainCamera;

        [Header("UI")]
        [SerializeField] private BuildingMenuUI buildingMenu;
        [SerializeField] private GameObject resourceDisplayPrefab; // Префаб для отображения ресурсов
        [SerializeField] private KeyCode buildMenuKey = KeyCode.B;
        [SerializeField] private KeyCode confirmPlacementKey = KeyCode.F;
        [SerializeField] private KeyCode cancelPlacementKey = KeyCode.Escape;
        [SerializeField] private KeyCode rotateKey = KeyCode.R;

        [Header("Visual Settings")]
        [SerializeField] private Material ghostMaterial; // Полупрозрачный материал
        [SerializeField] private LayerMask buildingLayer;

        // Состояние системы
        private bool isPlacingBuilding = false;
        private BuildingData currentBuildingData;
        private GameObject currentGhost; // Призрак здания
        private BuildingGhost ghostComponent;
        private int currentRotation = 0;
        private Vector3Int lastGridPosition;
        private bool canPlace = false;

        // Занятые тайлы
        private Dictionary<Vector3Int, GameObject> occupiedTiles = new Dictionary<Vector3Int, GameObject>();

        // Компоненты
        private Inventory playerInventory;
        private PlayerMovement playerMovement;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerInventory = player.GetComponent<Inventory>();
                playerMovement = player.GetComponent<PlayerMovement>();
            }

            if (buildingsContainer == null)
            {
                GameObject container = new GameObject("Buildings");
                buildingsContainer = container.transform;
            }
        }

        private void Update()
        {
            // Открытие меню строительства
            if (Input.GetKeyDown(buildMenuKey) && !isPlacingBuilding)
            {
                if (buildingMenu != null)
                    buildingMenu.OpenMenu();
            }

            // Обработка размещения здания
            if (isPlacingBuilding)
            {
                UpdateGhostPosition();
                HandlePlacementInput();
            }
        }

        /// <summary>
        /// Начать размещение здания
        /// </summary>
        public void StartPlacement(BuildingData buildingData)
        {
            if (buildingData == null) return;

            currentBuildingData = buildingData;
            isPlacingBuilding = true;
            currentRotation = 0;

            // Создаем призрак здания
            CreateGhost();

            // Блокируем движение игрока
            if (playerMovement != null)
                playerMovement.DisableMovement();

            Debug.Log($"[BuildingSystem] Started placing: {buildingData.buildingName}");
        }

        private void CreateGhost()
        {
            // Используем специальный префаб призрака или обычный префаб
            GameObject prefabToUse = currentBuildingData.ghostPrefab != null ?
                currentBuildingData.ghostPrefab : currentBuildingData.prefab;

            currentGhost = Instantiate(prefabToUse);
            currentGhost.name = "Ghost_" + currentBuildingData.buildingName;

            // Добавляем компонент призрака
            ghostComponent = currentGhost.AddComponent<BuildingGhost>();
            ghostComponent.Initialize(currentBuildingData, resourceDisplayPrefab);

            // Отключаем коллайдеры
            Collider2D[] colliders = currentGhost.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // Делаем полупрозрачным
            SetGhostTransparency(true);
        }

        private void SetGhostTransparency(bool isValid)
        {
            if (currentGhost == null) return;

            Color targetColor = isValid ?
                currentBuildingData.validPlacementColor :
                currentBuildingData.invalidPlacementColor;

            SpriteRenderer[] renderers = currentGhost.GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.color = targetColor;

                // Если есть специальный материал для призрака
                if (ghostMaterial != null)
                    renderer.material = ghostMaterial;
            }
        }

        private void UpdateGhostPosition()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // Конвертируем в позицию на сетке
            Vector3Int gridPos = mainTilemap.WorldToCell(mouseWorldPos);

            // Центрируем здание на сетке в зависимости от размера
            Vector3 worldPos = mainTilemap.GetCellCenterWorld(gridPos);

            // Корректируем позицию для зданий с четным размером
            if (currentBuildingData.size.x % 2 == 0)
                worldPos.x -= mainTilemap.cellSize.x / 2;
            if (currentBuildingData.size.y % 2 == 0)
                worldPos.y -= mainTilemap.cellSize.y / 2;

            currentGhost.transform.position = worldPos;

            // Проверяем валидность позиции
            if (gridPos != lastGridPosition)
            {
                lastGridPosition = gridPos;
                canPlace = IsValidPlacement(gridPos);
                SetGhostTransparency(canPlace);
                ghostComponent.UpdateResourceDisplay(canPlace);
            }
        }

        private void HandlePlacementInput()
        {
            // Поворот здания
            if (Input.GetKeyDown(rotateKey) && currentBuildingData.canRotate)
            {
                RotateGhost();
            }

            // Подтверждение размещения
            if (Input.GetKeyDown(confirmPlacementKey) && canPlace)
            {
                PlaceBuilding();
            }

            // Отмена размещения
            if (Input.GetKeyDown(cancelPlacementKey))
            {
                CancelPlacement();
            }
        }

        private void RotateGhost()
        {
            currentRotation = (currentRotation + 90) % 360;
            currentGhost.transform.rotation = Quaternion.Euler(0, 0, currentRotation);

            // Меняем размер местами при повороте на 90 или 270 градусов
            if (currentRotation == 90 || currentRotation == 270)
            {
                // Swap размеры для проверки
                var temp = currentBuildingData.size.x;
                currentBuildingData.size.x = currentBuildingData.size.y;
                currentBuildingData.size.y = temp;
            }

            // Перепроверяем валидность
            canPlace = IsValidPlacement(lastGridPosition);
            SetGhostTransparency(canPlace);
        }

        private bool IsValidPlacement(Vector3Int gridPos)
        {
            // Проверяем все тайлы, которые займет здание
            for (int x = 0; x < currentBuildingData.size.x; x++)
            {
                for (int y = 0; y < currentBuildingData.size.y; y++)
                {
                    Vector3Int checkPos = new Vector3Int(gridPos.x + x, gridPos.y + y, 0);

                    // Проверяем занятость
                    if (occupiedTiles.ContainsKey(checkPos))
                        return false;

                    // Проверяем наличие тайла (можно строить только на существующих тайлах)
                    if (mainTilemap.GetTile(checkPos) == null)
                        return false;

                    // TODO: Дополнительные проверки (тип поверхности и т.д.)
                }
            }

            return true;
        }

        private void PlaceBuilding()
        {
            if (!canPlace || currentBuildingData == null) return;

            // Проверяем ресурсы еще раз
            if (!currentBuildingData.CanAfford(playerInventory))
            {
                Debug.Log("[BuildingSystem] Not enough resources!");
                // TODO: Показать сообщение игроку
                return;
            }

            // Создаем строительную площадку
            GameObject construction = Instantiate(currentGhost);
            construction.name = "Construction_" + currentBuildingData.buildingName;
            construction.transform.SetParent(buildingsContainer);
            construction.transform.position = currentGhost.transform.position;
            construction.transform.rotation = currentGhost.transform.rotation;

            // Удаляем компонент призрака
            BuildingGhost ghost = construction.GetComponent<BuildingGhost>();
            if (ghost != null)
                Destroy(ghost);

            // Добавляем компонент строительства
            ConstructionSite site = construction.AddComponent<ConstructionSite>();
            site.Initialize(currentBuildingData, playerInventory);
            site.OnConstructionComplete += OnBuildingComplete;

            // Занимаем тайлы
            Vector3Int basePos = lastGridPosition;
            for (int x = 0; x < currentBuildingData.size.x; x++)
            {
                for (int y = 0; y < currentBuildingData.size.y; y++)
                {
                    Vector3Int tilePos = new Vector3Int(basePos.x + x, basePos.y + y, 0);
                    occupiedTiles[tilePos] = construction;
                }
            }

            // Завершаем размещение
            EndPlacement();
        }

        private void OnBuildingComplete(GameObject building, BuildingData data)
        {
            Debug.Log($"[BuildingSystem] Building complete: {data.buildingName}");

            // Активируем коллайдеры
            Collider2D[] colliders = building.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                col.enabled = true;
            }

            // Добавляем компонент здания
            BuildingController controller = building.AddComponent<BuildingController>();
            controller.Initialize(data);
        }

        private void CancelPlacement()
        {
            EndPlacement();
        }

        private void EndPlacement()
        {
            isPlacingBuilding = false;
            currentBuildingData = null;

            if (currentGhost != null)
            {
                Destroy(currentGhost);
                currentGhost = null;
            }

            // Возвращаем движение игроку
            if (playerMovement != null)
                playerMovement.EnableMovement();
        }

        /// <summary>
        /// Получить здание в указанной позиции
        /// </summary>
        public GameObject GetBuildingAtPosition(Vector3Int gridPos)
        {
            return occupiedTiles.ContainsKey(gridPos) ? occupiedTiles[gridPos] : null;
        }
    }
}