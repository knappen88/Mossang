using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using DG.Tweening;
using Building.UI;

namespace Building
{
    public class BuildingSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Tilemap mainTilemap;
        [SerializeField] private Tilemap buildingTilemap;
        [SerializeField] private Transform buildingsContainer;
        [SerializeField] private Camera mainCamera;

        [Header("UI")]
        [SerializeField] private BuildingMenuUI buildingMenu;
        [SerializeField] private GameObject resourceDisplayPrefab;
        [SerializeField] private KeyCode buildMenuKey = KeyCode.B;
        [SerializeField] private KeyCode confirmPlacementKey = KeyCode.F;
        [SerializeField] private KeyCode cancelPlacementKey = KeyCode.Escape;
        [SerializeField] private KeyCode rotateKey = KeyCode.R;

        [Header("Visual Settings")]
        [SerializeField] private Material ghostMaterial;
        [SerializeField] private LayerMask buildingLayer;

        private bool isPlacingBuilding = false;
        private BuildingData currentBuildingData;
        private GameObject currentGhost;
        private BuildingGhost ghostComponent;
        private int currentRotation = 0;
        private Vector3Int lastGridPosition;
        private bool canPlace = false;

        private Dictionary<Vector3Int, GameObject> occupiedTiles = new Dictionary<Vector3Int, GameObject>();

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
            if (Input.GetKeyDown(buildMenuKey) && !isPlacingBuilding)
            {
                if (buildingMenu != null)
                    buildingMenu.OpenMenu();
            }

            if (isPlacingBuilding)
            {
                UpdateGhostPosition();
                HandlePlacementInput();
            }
        }

        public void StartPlacement(BuildingData buildingData)
        {
            if (buildingData == null) return;

            currentBuildingData = buildingData;
            isPlacingBuilding = true;
            currentRotation = 0;

            CreateGhost();

            if (playerMovement != null)
                playerMovement.DisableMovement();

            Debug.Log($"[BuildingSystem] Started placing: {buildingData.buildingName}");
        }

        private void CreateGhost()
        {
            GameObject prefabToUse = currentBuildingData.ghostPrefab != null ?
                currentBuildingData.ghostPrefab : currentBuildingData.prefab;

            currentGhost = Instantiate(prefabToUse);
            currentGhost.name = "Ghost_" + currentBuildingData.buildingName;

            ghostComponent = currentGhost.AddComponent<BuildingGhost>();
            ghostComponent.Initialize(currentBuildingData, resourceDisplayPrefab);

            Collider2D[] colliders = currentGhost.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

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

                if (ghostMaterial != null)
                    renderer.material = ghostMaterial;
            }
        }

        private void UpdateGhostPosition()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            Vector3Int gridPos = mainTilemap.WorldToCell(mouseWorldPos);

            Vector3 worldPos = mainTilemap.GetCellCenterWorld(gridPos);

            if (currentBuildingData.size.x % 2 == 0)
                worldPos.x -= mainTilemap.cellSize.x / 2;
            if (currentBuildingData.size.y % 2 == 0)
                worldPos.y -= mainTilemap.cellSize.y / 2;

            currentGhost.transform.position = worldPos;

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
            if (Input.GetKeyDown(rotateKey) && currentBuildingData.canRotate)
            {
                RotateGhost();
            }

            if (Input.GetKeyDown(confirmPlacementKey) && canPlace)
            {
                PlaceBuilding();
            }

            if (Input.GetKeyDown(cancelPlacementKey))
            {
                CancelPlacement();
            }
        }

        private void RotateGhost()
        {
            currentRotation = (currentRotation + 90) % 360;
            currentGhost.transform.rotation = Quaternion.Euler(0, 0, currentRotation);

            if (currentRotation == 90 || currentRotation == 270)
            {
                var temp = currentBuildingData.size.x;
                currentBuildingData.size.x = currentBuildingData.size.y;
                currentBuildingData.size.y = temp;
            }

            canPlace = IsValidPlacement(lastGridPosition);
            SetGhostTransparency(canPlace);
        }

        private bool IsValidPlacement(Vector3Int gridPos)
        {
            for (int x = 0; x < currentBuildingData.size.x; x++)
            {
                for (int y = 0; y < currentBuildingData.size.y; y++)
                {
                    Vector3Int checkPos = new Vector3Int(gridPos.x + x, gridPos.y + y, 0);

                    if (occupiedTiles.ContainsKey(checkPos))
                        return false;

                    if (mainTilemap.GetTile(checkPos) == null)
                        return false;
                }
            }

            return true;
        }

        private void PlaceBuilding()
        {
            if (!canPlace || currentBuildingData == null) return;

            if (!currentBuildingData.CanAfford(playerInventory))
            {
                Debug.Log("[BuildingSystem] Not enough resources!");
                return;
            }

            GameObject construction = Instantiate(currentGhost);
            construction.name = "Construction_" + currentBuildingData.buildingName;
            construction.transform.SetParent(buildingsContainer);
            construction.transform.position = currentGhost.transform.position;
            construction.transform.rotation = currentGhost.transform.rotation;

            BuildingGhost ghost = construction.GetComponent<BuildingGhost>();
            if (ghost != null)
                Destroy(ghost);

            ConstructionSite site = construction.AddComponent<ConstructionSite>();
            site.Initialize(currentBuildingData, playerInventory);
            site.OnConstructionComplete += OnBuildingComplete;

            Vector3Int basePos = lastGridPosition;
            for (int x = 0; x < currentBuildingData.size.x; x++)
            {
                for (int y = 0; y < currentBuildingData.size.y; y++)
                {
                    Vector3Int tilePos = new Vector3Int(basePos.x + x, basePos.y + y, 0);
                    occupiedTiles[tilePos] = construction;
                }
            }

            EndPlacement();
        }

        private void OnBuildingComplete(GameObject building, BuildingData data)
        {
            Debug.Log($"[BuildingSystem] Building complete: {data.buildingName}");

            Collider2D[] colliders = building.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                col.enabled = true;
            }

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

            if (playerMovement != null)
                playerMovement.EnableMovement();
        }

        public GameObject GetBuildingAtPosition(Vector3Int gridPos)
        {
            return occupiedTiles.ContainsKey(gridPos) ? occupiedTiles[gridPos] : null;
        }
    }
}