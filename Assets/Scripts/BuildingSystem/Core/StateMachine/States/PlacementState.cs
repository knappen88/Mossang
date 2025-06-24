using UnityEngine;
using BuildingSystem.Core.Events;
using BuildingSystem.Components;
using BuildingSystem.Controllers;
using BuildingSystem.Core;

namespace BuildingSystem.Core.StateMachine
{
    public class PlacementState : BuildingSystemStateBase
    {
        public PlacementState(BuildingSystemContext context) : base(context) { }

        public override void Enter()
        {
            if (context.CurrentBuildingData == null)
            {
                Debug.LogError("No building data set for placement!");
                return;
            }

            // Create ghost
            context.CurrentGhost = context.BuildingFactory.CreateGhost(context.CurrentBuildingData);

            // Show grid if configured
            if (context.Config.ShowGridOnPlacement)
            {
                ShowGrid();
            }

            // Publish event
            context.EventChannel.Publish(new BuildingPlacementStartedEvent(context.CurrentBuildingData));
        }

        public override void Exit()
        {
            if (context.CurrentGhost != null)
            {
                context.BuildingFactory.RecycleGhost(context.CurrentGhost);
                context.CurrentGhost = null;
            }

            HideGrid();
        }

        public override void Update()
        {
            UpdateGhostPosition();
            UpdateGhostVisual();
        }

        public override void HandleInput()
        {
            if (Input.GetKeyDown(context.Config.ConfirmPlacementKey))
            {
                TryPlaceBuilding();
            }
            else if (Input.GetKeyDown(context.Config.CancelPlacementKey))
            {
                CancelPlacement();
            }
            else if (Input.GetKeyDown(context.Config.RotateKey))
            {
                RotateGhost();
            }
        }

        private void UpdateGhostPosition()
        {
            var mouseWorldPos = context.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            var gridPos = context.GridManager.WorldToGrid(mouseWorldPos);
            var worldPos = context.GridManager.GridToWorld(gridPos);

            context.CurrentGridPosition = gridPos;
            context.CurrentGhost.transform.position = worldPos;

            // Update placement validity
            var validationResult = context.PlacementValidator.ValidatePlacement(gridPos, context.CurrentBuildingData);
            context.CanPlace = validationResult.IsValid;
        }

        private void UpdateGhostVisual()
        {
            var ghostComponent = context.CurrentGhost.GetComponent<BuildingGhostComponent>();
            if (ghostComponent != null)
            {
                ghostComponent.SetValid(context.CanPlace);
            }
        }

        private void TryPlaceBuilding()
        {
            if (!context.CanPlace)
            {
                var validationResult = context.PlacementValidator.ValidatePlacement(
                    context.CurrentGridPosition,
                    context.CurrentBuildingData);

                context.EventChannel.Publish(new PlacementValidationFailedEvent(
                    validationResult.ErrorType,
                    validationResult.ErrorMessage));
                return;
            }

            // Check resources
            if (!context.ResourceManager.HasResources(context.CurrentBuildingData.ResourceRequirements))
            {
                context.EventChannel.Publish(new PlacementValidationFailedEvent(
                    PlacementErrorType.InsufficientResources,
                    "Not enough resources"));
                return;
            }

            // Place building
            PlaceBuilding();
        }

        private void PlaceBuilding()
        {
            // Create building
            var building = context.BuildingFactory.CreateBuilding(
                context.CurrentBuildingData,
                context.CurrentGhost.transform.position,
                context.CurrentGhost.transform.rotation);

            // Register in repository
            context.BuildingRepository.RegisterBuilding(
                building,
                context.CurrentBuildingData,
                context.CurrentGridPosition);

            // Occupy grid cells
            context.GridManager.OccupyCells(
                context.CurrentGridPosition,
                context.CurrentBuildingData.Size,
                building);

            // Consume resources
            context.ResourceManager.ConsumeResources(context.CurrentBuildingData.ResourceRequirements);

            // Start construction
            context.ConstructionManager.StartConstruction(building, context.CurrentBuildingData);

            // Publish event
            context.EventChannel.Publish(new BuildingPlacedEvent(
                building,
                context.CurrentBuildingData,
                context.CurrentGridPosition));

            // Clear placement data
            context.CurrentBuildingData = null;
        }

        private void CancelPlacement()
        {
            context.EventChannel.Publish(new BuildingPlacementCancelledEvent(context.CurrentBuildingData));
            context.CurrentBuildingData = null;
        }

        private void RotateGhost()
        {
            context.CurrentRotation = (context.CurrentRotation + 90) % 360;
            context.CurrentGhost.transform.rotation = Quaternion.Euler(0, 0, context.CurrentRotation);
        }

        private void ShowGrid()
        {
            // Simple grid visualization
            Debug.Log("Grid visualization enabled");
            // You can implement actual grid visualization here
            // For example, spawn grid cell prefabs or enable a grid overlay
        }

        private void HideGrid()
        {
            // Hide grid visualization
            Debug.Log("Grid visualization disabled");
            // Disable grid overlay or destroy grid cell objects
        }
    }
}