using BuildingSystem.Core.Interfaces;
using BuildingSystem.Core.Events;
using UnityEngine;
namespace BuildingSystem.Core.StateMachine
{
    public class BuildingSystemContext
    {
        public IGridManager GridManager { get; }
        public IPlacementValidator PlacementValidator { get; }
        public IBuildingFactory BuildingFactory { get; }
        public IBuildingRepository BuildingRepository { get; }
        public IConstructionManager ConstructionManager { get; }
        public IResourceManager ResourceManager { get; }
        public BuildingEventChannel EventChannel { get; }
        public Camera MainCamera { get; }
        public Transform BuildingContainer { get; }
        public BuildingSystemConfig Config { get; }

        // Current placement data
        public BuildingData CurrentBuildingData { get; set; }
        public GameObject CurrentGhost { get; set; }
        public Vector3Int CurrentGridPosition { get; set; }
        public int CurrentRotation { get; set; }
        public bool CanPlace { get; set; }

        public BuildingSystemContext(
            IGridManager gridManager,
            IPlacementValidator placementValidator,
            IBuildingFactory buildingFactory,
            IBuildingRepository buildingRepository,
            IConstructionManager constructionManager,
            IResourceManager resourceManager,
            BuildingEventChannel eventChannel,
            Camera mainCamera,
            Transform buildingContainer,
            BuildingSystemConfig config)
        {
            GridManager = gridManager;
            PlacementValidator = placementValidator;
            BuildingFactory = buildingFactory;
            BuildingRepository = buildingRepository;
            ConstructionManager = constructionManager;
            ResourceManager = resourceManager;
            EventChannel = eventChannel;
            MainCamera = mainCamera;
            BuildingContainer = buildingContainer;
            Config = config;
        }
    }
}