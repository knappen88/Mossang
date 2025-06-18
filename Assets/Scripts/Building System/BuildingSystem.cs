using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using DG.Tweening;
using Building.UI;

namespace Building
{
    public class BuildingSystem : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private Tilemap mainTilemap;
        [SerializeField] private Tilemap buildingTilemap;
        [SerializeField] private Transform buildingsContainer;
        [SerializeField] private Camera mainCamera;

        [Header("UI")]
        [SerializeField] private BuildingMenuUI buildingMenu;
        [SerializeField] private GameObject resourceDisplayPrefab;
        [SerializeField] private GameObject constructionUIPrefab;

        [Header("Controls")]
        [SerializeField] private KeyCode buildMenuKey = KeyCode.B;
        [SerializeField] private KeyCode confirmPlacementKey = KeyCode.F;
        [SerializeField] private KeyCode cancelPlacementKey = KeyCode.Escape;
        [SerializeField] private KeyCode rotateKey = KeyCode.R;

        [Header("Visual Settings")]
        [SerializeField] private Material ghostMaterial;
        [SerializeField] private float ghostValidAlpha = 0.6f;
        [SerializeField] private float ghostInvalidAlpha = 0.3f;
        [SerializeField] private Color validPlacementTint = Color.green;
        [SerializeField] private Color invalidPlacementTint = Color.red;

        [Header("Grid Visualization")]
        [SerializeField] private bool showGridOnPlacement = true;
        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private float gridLineWidth = 0.02f;
        [SerializeField] private int gridRadius = 10;
        [SerializeField] private Material gridMaterial;
        [SerializeField] private float gridFadeInDuration = 0.5f;
        [SerializeField] private float gridFadeOutDuration = 0.3f;

        [Header("Audio")]
        [SerializeField] private AudioClip placementSound;
        [SerializeField] private AudioClip rotateSound;
        [SerializeField] private AudioClip cancelSound;
        [SerializeField] private AudioClip errorSound;

        #endregion

        #region Private Fields

        // State
        private bool isPlacingBuilding = false;
        private BuildingData currentBuildingData;
        private BuildingData originalBuildingData; // Для сохранения оригинального размера при вращении
        private GameObject currentGhost;
        private BuildingGhost ghostComponent;
        private int currentRotation = 0;
        private Vector3Int lastGridPosition;
        private bool canPlace = false;

        // Grid Visualization
        private GameObject gridVisualization;
        private List<LineRenderer> gridLines = new List<LineRenderer>();
        private List<Tween> activeGridTweens = new List<Tween>();

        // Building Registry
        private Dictionary<Vector3Int, GameObject> occupiedTiles = new Dictionary<Vector3Int, GameObject>();
        private List<GameObject> allBuildings = new List<GameObject>();

        // Cached References
        private Inventory playerInventory;
        private PlayerMovement playerMovement;
        private AudioSource audioSource;

        #endregion

        #region Events

        public System.Action<BuildingData> OnBuildingPlacementStarted;
        public System.Action OnBuildingPlacementCancelled;
        public System.Action<GameObject, BuildingData> OnBuildingPlaced;
        public System.Action<GameObject, BuildingData> OnBuildingCompleted;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeReferences();
            ValidateComponents();
        }

        private void Update()
        {
            HandleInput();

            if (isPlacingBuilding)
            {
                UpdatePlacement();
            }
        }

        private void OnDestroy()
        {
            CleanupTweens();
        }

        #endregion

        #region Initialization

        private void InitializeReferences()
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

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void ValidateComponents()
        {
            if (mainTilemap == null)
                Debug.LogError("[BuildingSystem] Main Tilemap not assigned!");

            if (buildingMenu == null)
                Debug.LogWarning("[BuildingSystem] Building Menu UI not assigned!");

            if (resourceDisplayPrefab == null)
                Debug.LogWarning("[BuildingSystem] Resource Display Prefab not assigned!");

            if (constructionUIPrefab == null)
                Debug.LogWarning("[BuildingSystem] Construction UI Prefab not assigned!");
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            // Open building menu
            if (Input.GetKeyDown(buildMenuKey) && !isPlacingBuilding)
            {
                OpenBuildingMenu();
            }

            // Placement controls
            if (isPlacingBuilding)
            {
                if (Input.GetKeyDown(rotateKey))
                    RotateGhost();

                if (Input.GetKeyDown(confirmPlacementKey))
                    TryPlaceBuilding();

                if (Input.GetKeyDown(cancelPlacementKey))
                    CancelPlacement();
            }
        }

        private void OpenBuildingMenu()
        {
            if (buildingMenu != null)
            {
                buildingMenu.OpenMenu();
            }
            else
            {
                Debug.LogWarning("[BuildingSystem] Building menu not assigned!");
            }
        }

        #endregion

        #region Placement System

        public void StartPlacement(BuildingData buildingData)
        {
            if (buildingData == null)
            {
                Debug.LogError("[BuildingSystem] Cannot start placement with null BuildingData!");
                return;
            }

            // Cancel any existing placement
            if (isPlacingBuilding)
            {
                CancelPlacement();
            }

            // Store original data for rotation
            originalBuildingData = buildingData;
            currentBuildingData = Instantiate(buildingData); // Create a copy to modify

            isPlacingBuilding = true;
            currentRotation = 0;

            CreateGhost();

            if (showGridOnPlacement)
                CreateGridVisualization();

            if (playerMovement != null)
                playerMovement.DisableMovement();

            OnBuildingPlacementStarted?.Invoke(buildingData);

            Debug.Log($"[BuildingSystem] Started placing: {buildingData.buildingName}");
        }

        private void UpdatePlacement()
        {
            UpdateGhostPosition();
            UpdateGhostVisual();
        }

        private void TryPlaceBuilding()
        {
            if (!canPlace)
            {
                PlaySound(errorSound);
                ShowPlacementError("Cannot place here!");
                return;
            }

            if (!currentBuildingData.CanAfford(playerInventory))
            {
                PlaySound(errorSound);
                ShowPlacementError("Not enough resources!");
                return;
            }

            PlaceBuilding();
        }

        private void PlaceBuilding()
        {
            // Create building instance
            GameObject building = Instantiate(currentBuildingData.prefab);
            building.name = $"Construction_{currentBuildingData.buildingName}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            building.transform.SetParent(buildingsContainer);
            building.transform.position = currentGhost.transform.position;
            building.transform.rotation = currentGhost.transform.rotation;

            // Reset sprite colors
            SpriteRenderer[] renderers = building.GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.color = Color.white;
            }

            // Add construction site
            ConstructionSite site = building.AddComponent<ConstructionSite>();
            site.Initialize(currentBuildingData, playerInventory, constructionUIPrefab); // Передаем префаб!
            site.OnConstructionComplete += OnBuildingComplete;

            // Pass the construction UI prefab if available
            if (constructionUIPrefab != null)
            {
                // You might need to modify ConstructionSite to accept the prefab
                // For now, it should work if the prefab is assigned in BuildingData
            }

            site.Initialize(currentBuildingData, playerInventory);
            site.OnConstructionComplete += OnBuildingComplete;

            // Register occupied tiles
            RegisterOccupiedTiles(building, lastGridPosition);

            // Track building
            allBuildings.Add(building);

            // Play sound
            PlaySound(placementSound);

            // Fire event
            OnBuildingPlaced?.Invoke(building, currentBuildingData);

            // End placement
            EndPlacement();
        }

        private void CancelPlacement()
        {
            PlaySound(cancelSound);
            OnBuildingPlacementCancelled?.Invoke();
            EndPlacement();
        }

        private void EndPlacement()
        {
            isPlacingBuilding = false;
            currentBuildingData = null;
            originalBuildingData = null;

            if (currentGhost != null)
            {
                Destroy(currentGhost);
                currentGhost = null;
                ghostComponent = null;
            }

            if (showGridOnPlacement)
                DestroyGridVisualization();

            if (playerMovement != null)
                playerMovement.EnableMovement();
        }

        #endregion

        #region Ghost Management

        private void CreateGhost()
        {
            GameObject prefabToUse = currentBuildingData.ghostPrefab != null ?
                currentBuildingData.ghostPrefab : currentBuildingData.prefab;

            currentGhost = Instantiate(prefabToUse);
            currentGhost.name = $"Ghost_{currentBuildingData.buildingName}";

            // Remove any building controller
            BuildingController controller = currentGhost.GetComponent<BuildingController>();
            if (controller != null)
                Destroy(controller);

            // Add ghost component
            ghostComponent = currentGhost.AddComponent<BuildingGhost>();
            ghostComponent.Initialize(currentBuildingData, resourceDisplayPrefab);

            // Disable colliders
            Collider2D[] colliders = currentGhost.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }
        }

        private void UpdateGhostPosition()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            Vector3Int gridPos = mainTilemap.WorldToCell(mouseWorldPos);
            Vector3 worldPos = mainTilemap.GetCellCenterWorld(gridPos);

            // Adjust for even-sized buildings
            if (currentBuildingData.size.x % 2 == 0)
                worldPos.x -= mainTilemap.cellSize.x / 2;
            if (currentBuildingData.size.y % 2 == 0)
                worldPos.y -= mainTilemap.cellSize.y / 2;

            worldPos.z = 0;
            currentGhost.transform.position = worldPos;

            // Check validity if position changed
            if (gridPos != lastGridPosition)
            {
                lastGridPosition = gridPos;
                canPlace = IsValidPlacement(gridPos);
                ghostComponent?.UpdateResourceDisplay(canPlace);
            }
        }

        private void UpdateGhostVisual()
        {
            if (currentGhost == null) return;

            Color targetColor = canPlace ? validPlacementTint : invalidPlacementTint;
            float targetAlpha = canPlace ? ghostValidAlpha : ghostInvalidAlpha;

            SpriteRenderer[] renderers = currentGhost.GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                Color finalColor = targetColor;
                finalColor.a = targetAlpha;
                renderer.color = finalColor;

                if (ghostMaterial != null)
                    renderer.material = ghostMaterial;
            }
        }

        private void RotateGhost()
        {
            if (!currentBuildingData.canRotate) return;

            currentRotation = (currentRotation + 90) % 360;
            currentGhost.transform.rotation = Quaternion.Euler(0, 0, currentRotation);

            // Swap dimensions for 90/270 degree rotations
            if (currentRotation == 90 || currentRotation == 270)
            {
                currentBuildingData.size = new Vector2Int(
                    originalBuildingData.size.y,
                    originalBuildingData.size.x
                );
            }
            else
            {
                currentBuildingData.size = originalBuildingData.size;
            }

            PlaySound(rotateSound);

            // Recheck placement validity
            canPlace = IsValidPlacement(lastGridPosition);
        }

        #endregion

        #region Placement Validation

        private bool IsValidPlacement(Vector3Int gridPos)
        {
            // Check each tile the building would occupy
            for (int x = 0; x < currentBuildingData.size.x; x++)
            {
                for (int y = 0; y < currentBuildingData.size.y; y++)
                {
                    Vector3Int checkPos = new Vector3Int(gridPos.x + x, gridPos.y + y, 0);

                    // Check if tile is already occupied
                    if (occupiedTiles.ContainsKey(checkPos))
                        return false;

                    // Check if tile exists
                    if (mainTilemap.GetTile(checkPos) == null)
                        return false;

                    // Additional checks can be added here
                    // e.g., check for specific tile types, terrain restrictions, etc.
                }
            }

            return true;
        }

        private void RegisterOccupiedTiles(GameObject building, Vector3Int basePos)
        {
            for (int x = 0; x < currentBuildingData.size.x; x++)
            {
                for (int y = 0; y < currentBuildingData.size.y; y++)
                {
                    Vector3Int tilePos = new Vector3Int(basePos.x + x, basePos.y + y, 0);
                    occupiedTiles[tilePos] = building;
                }
            }
        }

        #endregion

        #region Grid Visualization

        private void CreateGridVisualization()
        {
            gridVisualization = new GameObject("Grid Visualization");
            gridVisualization.transform.SetParent(transform);

            Vector3 cameraPos = mainCamera.transform.position;
            cameraPos.z = 0;
            Vector3Int centerCell = mainTilemap.WorldToCell(cameraPos);

            // Create horizontal lines
            for (int y = -gridRadius; y <= gridRadius; y++)
            {
                CreateGridLine(
                    new Vector3Int(centerCell.x - gridRadius, centerCell.y + y, 0),
                    new Vector3Int(centerCell.x + gridRadius, centerCell.y + y, 0),
                    $"GridLine_H_{y}"
                );
            }

            // Create vertical lines
            for (int x = -gridRadius; x <= gridRadius; x++)
            {
                CreateGridLine(
                    new Vector3Int(centerCell.x + x, centerCell.y - gridRadius, 0),
                    new Vector3Int(centerCell.x + x, centerCell.y + gridRadius, 0),
                    $"GridLine_V_{x}"
                );
            }

            AnimateGridAppearance();
        }

        private void CreateGridLine(Vector3Int startCell, Vector3Int endCell, string name)
        {
            GameObject lineGO = new GameObject(name);
            lineGO.transform.SetParent(gridVisualization.transform);

            LineRenderer line = lineGO.AddComponent<LineRenderer>();

            // Setup material
            if (gridMaterial != null)
                line.material = gridMaterial;
            else
                line.material = new Material(Shader.Find("Sprites/Default"));

            // Setup appearance
            line.startColor = gridColor;
            line.endColor = gridColor;
            line.startWidth = gridLineWidth;
            line.endWidth = gridLineWidth;
            line.positionCount = 2;

            // Calculate positions
            Vector3 startPos = mainTilemap.GetCellCenterWorld(startCell);
            Vector3 endPos = mainTilemap.GetCellCenterWorld(endCell);

            startPos.z = -0.5f;
            endPos.z = -0.5f;

            // Adjust for grid alignment
            float halfCell = mainTilemap.cellSize.x / 2f;

            if (startCell.y == endCell.y) // Horizontal line
            {
                startPos.y -= halfCell;
                endPos.y -= halfCell;
                startPos.x -= halfCell;
                endPos.x += halfCell;
            }
            else // Vertical line
            {
                startPos.x -= halfCell;
                endPos.x -= halfCell;
                startPos.y -= halfCell;
                endPos.y += halfCell;
            }

            line.SetPosition(0, startPos);
            line.SetPosition(1, endPos);

            // Setup sorting
            line.sortingLayerName = "Default";
            line.sortingOrder = 10;
            line.useWorldSpace = true;
            line.alignment = LineAlignment.TransformZ;

            gridLines.Add(line);
        }

        private void AnimateGridAppearance()
        {
            foreach (var line in gridLines)
            {
                Color startColor = gridColor;
                startColor.a = 0;
                line.startColor = startColor;
                line.endColor = startColor;

                Tween tween = DOVirtual.Float(0, gridColor.a, gridFadeInDuration, (value) =>
                {
                    Color newColor = gridColor;
                    newColor.a = value;
                    line.startColor = newColor;
                    line.endColor = newColor;
                });

                activeGridTweens.Add(tween);
            }
        }

        private void DestroyGridVisualization()
        {
            if (gridVisualization == null) return;

            // Cancel active tweens
            CleanupTweens();

            // Animate fade out
            foreach (var line in gridLines)
            {
                if (line != null)
                {
                    Tween tween = DOVirtual.Float(line.startColor.a, 0, gridFadeOutDuration, (value) =>
                    {
                        Color newColor = line.startColor;
                        newColor.a = value;
                        line.startColor = newColor;
                        line.endColor = newColor;
                    }).OnComplete(() => {
                        if (line != null)
                            Destroy(line.gameObject);
                    });

                    activeGridTweens.Add(tween);
                }
            }

            DOVirtual.DelayedCall(gridFadeOutDuration + 0.1f, () =>
            {
                if (gridVisualization != null)
                    Destroy(gridVisualization);

                gridLines.Clear();
                CleanupTweens();
            });
        }

        private void CleanupTweens()
        {
            foreach (var tween in activeGridTweens)
            {
                if (tween != null && tween.IsActive())
                    tween.Kill();
            }
            activeGridTweens.Clear();
        }

        #endregion

        #region Building Completion

        private void OnBuildingComplete(GameObject building, BuildingData data)
        {
            Debug.Log($"[BuildingSystem] Building complete: {data.buildingName}");

            // Enable colliders
            Collider2D[] colliders = building.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                col.enabled = true;
            }

            // Add building controller
            BuildingController controller = building.GetComponent<BuildingController>();
            if (controller == null)
            {
                controller = building.AddComponent<BuildingController>();
            }
            controller.Initialize(data);

            // Fire completion event
            OnBuildingCompleted?.Invoke(building, data);
        }

        #endregion

        #region Utility Methods

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private void ShowPlacementError(string message)
        {
            // You can implement UI feedback here
            Debug.LogWarning($"[BuildingSystem] Placement Error: {message}");

            // Example: Show floating text or UI notification
            // NotificationManager.ShowNotification(message, NotificationType.Error);
        }

        public GameObject GetBuildingAtPosition(Vector3Int gridPos)
        {
            return occupiedTiles.ContainsKey(gridPos) ? occupiedTiles[gridPos] : null;
        }

        public List<GameObject> GetAllBuildings()
        {
            return new List<GameObject>(allBuildings);
        }

        public void RemoveBuilding(GameObject building)
        {
            if (building == null) return;

            // Remove from occupied tiles
            List<Vector3Int> tilesToRemove = new List<Vector3Int>();
            foreach (var kvp in occupiedTiles)
            {
                if (kvp.Value == building)
                    tilesToRemove.Add(kvp.Key);
            }

            foreach (var tile in tilesToRemove)
            {
                occupiedTiles.Remove(tile);
            }

            // Remove from buildings list
            allBuildings.Remove(building);

            // Destroy the building
            Destroy(building);
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // Draw occupied tiles
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            foreach (var kvp in occupiedTiles)
            {
                Vector3 worldPos = mainTilemap.GetCellCenterWorld(kvp.Key);
                Gizmos.DrawCube(worldPos, mainTilemap.cellSize * 0.9f);
            }

            // Draw current placement area
            if (isPlacingBuilding && currentGhost != null)
            {
                Gizmos.color = canPlace ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);

                for (int x = 0; x < currentBuildingData.size.x; x++)
                {
                    for (int y = 0; y < currentBuildingData.size.y; y++)
                    {
                        Vector3Int tilePos = new Vector3Int(lastGridPosition.x + x, lastGridPosition.y + y, 0);
                        Vector3 worldPos = mainTilemap.GetCellCenterWorld(tilePos);
                        Gizmos.DrawWireCube(worldPos, mainTilemap.cellSize * 0.95f);
                    }
                }
            }
        }

        #endregion
    }
}