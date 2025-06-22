using UnityEngine;
namespace BuildingSystem.Core.Interfaces
{
    public interface IConstructionManager
    {
        void StartConstruction(GameObject building, BuildingData data);
        void UpdateConstruction(GameObject building, float deltaTime);
        void CompleteConstruction(GameObject building);
        void CancelConstruction(GameObject building);
        float GetConstructionProgress(GameObject building);
        bool IsUnderConstruction(GameObject building);
    }
}
