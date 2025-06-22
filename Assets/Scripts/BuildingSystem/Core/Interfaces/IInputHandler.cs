using UnityEngine;

namespace BuildingSystem.Core.Interfaces
{
    public interface IInputHandler
{
    void Enable();
    void Disable();
    bool IsEnabled { get; }
}
    }
