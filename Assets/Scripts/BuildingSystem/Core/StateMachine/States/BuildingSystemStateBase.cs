
namespace BuildingSystem.Core.StateMachine
{
    public abstract class BuildingSystemStateBase : IState
    {
        protected readonly BuildingSystemContext context;

        protected BuildingSystemStateBase(BuildingSystemContext context)
        {
            this.context = context;
        }

        public abstract void Enter();
        public abstract void Exit();
        public abstract void Update();
        public abstract void HandleInput();
    }
}