
namespace BuildingSystem.Controllers
{
    public class BuildingInputHandler : IInputHandler
    {
        private readonly BuildingSystemController controller;
        private readonly BuildingSystemConfig config;
        private bool isEnabled = true;

        public bool IsEnabled => isEnabled;

        public BuildingInputHandler(BuildingSystemController controller, BuildingSystemConfig config)
        {
            this.controller = controller;
            this.config = config;
        }

        public void Enable() => isEnabled = true;
        public void Disable() => isEnabled = false;

        public void HandleInput()
        {
            if (!isEnabled) return;

            // Open building menu
            if (Input.GetKeyDown(config.BuildMenuKey) && !controller.IsPlacingBuilding)
            {
                OpenBuildingMenu();
            }

            // Cancel current action
            if (Input.GetKeyDown(config.CancelPlacementKey))
            {
                controller.CancelCurrentAction();
            }

            // Start demolition
            if (Input.GetKeyDown(config.DemolishKey) && !controller.IsPlacingBuilding)
            {
                controller.StartDemolition();
            }
        }

        private void OpenBuildingMenu()
        {
            // Find and open building menu UI
            var menuUI = Object.FindObjectOfType<BuildingMenuView>();
            menuUI?.Show();
        }
    }
}