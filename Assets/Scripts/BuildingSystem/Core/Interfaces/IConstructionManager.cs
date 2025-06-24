using UnityEngine;

namespace BuildingSystem.Core.Interfaces
{
    public interface IConstructionManager
    {
        void StartConstruction(GameObject building, BuildingData data);
        void UpdateConstruction(GameObject building, float deltaTime);
        bool IsUnderConstruction(GameObject building);
        float GetConstructionProgress(GameObject building);
        void CompleteConstruction(GameObject building);
        void CancelConstruction(GameObject building);
    }
}