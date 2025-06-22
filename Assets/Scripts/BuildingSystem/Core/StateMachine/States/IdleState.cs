using BuildingSystem.Core.StateMachine;
using UnityEngine;
namespace BuildingSystem.Core.StateMachine
{
    public class IdleState : BuildingSystemStateBase
    {
        public IdleState(BuildingSystemContext context) : base(context) { }

        public override void Enter()
        {
            // Clean up any lingering state
            if (context.CurrentGhost != null)
            {
                context.BuildingFactory.RecycleGhost(context.CurrentGhost);
                context.CurrentGhost = null;
            }
        }

        public override void Exit() { }

        public override void Update() { }

        public override void HandleInput()
        {
            // Handle building selection, menu opening, etc.
            if (Input.GetMouseButtonDown(0))
            {
                HandleBuildingSelection();
            }
        }

        private void HandleBuildingSelection()
        {
            var ray = context.MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var building = hit.collider.GetComponent<BuildingController>();
                if (building != null)
                {
                    context.EventChannel.Publish(new BuildingSelectedEvent(building.gameObject));
                }
            }
        }
    }
}