using UnityEngine;

namespace BuildingSystem.Core.StateMachine
{
    public class IdleState : BuildingSystemStateBase
    {
        public IdleState(BuildingSystemContext context) : base(context) { }

        public override void Enter()
        {
            // Clear any temporary data
            context.CurrentBuildingData = null;
            context.CurrentGhost = null;
            context.CurrentGridPosition = Vector3Int.zero;
            context.CurrentRotation = 0;
        }

        public override void Exit() { }

        public override void Update()
        {
            // Update construction progress for all buildings
            foreach (var building in context.BuildingRepository.GetAllBuildings())
            {
                if (context.ConstructionManager.IsUnderConstruction(building))
                {
                    context.ConstructionManager.UpdateConstruction(building, Time.deltaTime);
                }
            }
        }

        public override void HandleInput()
        {
            // Input is handled by BuildingInputHandler in idle state
        }
    }
}