namespace BuildingSystem.Core.Interfaces
{
    public interface IInputHandler
    {
        void HandleInput();
        bool IsEnabled { get; }
        void Enable();
        void Disable();
    }
}