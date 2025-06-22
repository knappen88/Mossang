using UnityEngine;
using BuildingSystem.Core.Interfaces;
using BuildingSystem.Core.StateMachine;
using BuildingSystem.Core.Events;
using BuildingSystem.Systems;
using BuildingSystem.Config;

namespace BuildingSystem.Controllers
{
    public class BuildingSystemController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BuildingSystemConfig systemConfig;
        [SerializeField] private GridConfig gridConfig;
        [SerializeField] private BuildingEventChannel eventChannel;

        [Header("References")]
        [SerializeField] private Grid unityGrid;
        [SerializeField] private Transform buildingContainer;
        [SerializeField] private Camera mainCamera;

        // Systems
        private IGridManager gridManager;
        private IPlacementValidator placementValidator;
        private IBuildingFactory buildingFactory;
        private IBuildingRepository buildingRepository;
        private IConstructionManager constructionManager;
        private IResourceManager resourceManager;
        private IInputHandler inputHandler;

        // State Machine
        private StateMachine<BuildingSystemState> stateMachine;
        private BuildingSystemContext context;

        // Initialization
        private void Awake()
        {
            InitializeSystems();
            InitializeStateMachine();
            SubscribeToEvents();
        }

        private void InitializeSystems()
        {
            // Create systems
            gridManager = new GridManager(unityGrid, gridConfig);
            buildingFactory = new BuildingFactory(buildingContainer, systemConfig);
            buildingRepository = new BuildingRepository();

            // Get resource manager from player or game manager
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var inventory = player.GetComponent<IResourceManager>();
                if (inventory != null)
                {
                    resourceManager = inventory;
                }
                else
                {
                    // Create adapter for legacy inventory
                    resourceManager = new InventoryAdapter(player.GetComponent<Inventory>());
                }
            }

            placementValidator = new PlacementValidator(gridManager, gridConfig, resourceManager);
            constructionManager = new ConstructionManager(eventChannel, systemConfig);
            inputHandler = new BuildingInputHandler(this, systemConfig);

            // Setup camera if not assigned
            if (mainCamera == null)
                mainCamera = Camera.main;

            // Create building container if needed
            if (buildingContainer == null)
            {
                var container = new GameObject("Buildings");
                buildingContainer = container.transform;
            }
        }

        private void InitializeStateMachine()
        {
            // Create context
            context = new BuildingSystemContext(
                gridManager,
                placementValidator,
                buildingFactory,
                buildingRepository,
                constructionManager,
                resourceManager,
                eventChannel,
                mainCamera,
                buildingContainer,
                systemConfig
            );

            // Create state machine
            stateMachine = new StateMachine<BuildingSystemState>();

            // Register states
            stateMachine.RegisterState(BuildingSystemState.Idle, new IdleState(context));
            stateMachine.RegisterState(BuildingSystemState.PlacingBuilding, new PlacementState(context));
            stateMachine.RegisterState(BuildingSystemState.ConstructingBuilding, new ConstructionState(context));
            stateMachine.RegisterState(BuildingSystemState.DemolishingBuilding, new DemolitionState(context));

            // Start in idle state
            stateMachine.ChangeState(BuildingSystemState.Idle);
        }

        private void SubscribeToEvents()
        {
            eventChannel.Subscribe<BuildingPlacedEvent>(OnBuildingPlaced);
            eventChannel.Subscribe<BuildingPlacementCancelledEvent>(OnPlacementCancelled);
            eventChannel.Subscribe<BuildingConstructionCompletedEvent>(OnConstructionCompleted);
            eventChannel.Subscribe<BuildingSelectedEvent>(OnBuildingSelected);
        }

        private void OnDestroy()
        {
            eventChannel.Unsubscribe<BuildingPlacedEvent>(OnBuildingPlaced);
            eventChannel.Unsubscribe<BuildingPlacementCancelledEvent>(OnPlacementCancelled);
            eventChannel.Unsubscribe<BuildingConstructionCompletedEvent>(OnConstructionCompleted);
            eventChannel.Unsubscribe<BuildingSelectedEvent>(OnBuildingSelected);
        }

        // Update
        private void Update()
        {
            inputHandler.HandleInput();
            stateMachine.Update();
            stateMachine.HandleInput();
        }

        // Public API
        public void StartPlacement(BuildingData buildingData)
        {
            if (buildingData == null)
            {
                Debug.LogError("Cannot start placement with null building data!");
                return;
            }

            context.CurrentBuildingData = buildingData;
            stateMachine.ChangeState(BuildingSystemState.PlacingBuilding);
        }

        public void CancelCurrentAction()
        {
            stateMachine.ChangeState(BuildingSystemState.Idle);
        }

        public void StartDemolition()
        {
            stateMachine.ChangeState(BuildingSystemState.DemolishingBuilding);
        }

        // Event Handlers
        private void OnBuildingPlaced(BuildingPlacedEvent e)
        {
            stateMachine.ChangeState(BuildingSystemState.Idle);
        }

        private void OnPlacementCancelled(BuildingPlacementCancelledEvent e)
        {
            stateMachine.ChangeState(BuildingSystemState.Idle);
        }

        private void OnConstructionCompleted(BuildingConstructionCompletedEvent e)
        {
            // Update building state in repository
            var repo = buildingRepository as BuildingRepository;
            repo?.UpdateBuildingState(e.Building, BuildingState.Completed);
        }

        private void OnBuildingSelected(BuildingSelectedEvent e)
        {
            // Handle building selection
        }

        // Getters for UI
        public BuildingSystemState CurrentState => stateMachine.CurrentStateType;
        public bool IsPlacingBuilding => CurrentState == BuildingSystemState.PlacingBuilding;
        public BuildingData CurrentBuildingData => context.CurrentBuildingData;
    }