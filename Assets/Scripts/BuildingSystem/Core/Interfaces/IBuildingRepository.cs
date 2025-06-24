using UnityEngine;
using System.Collections.Generic;
using BuildingSystem.Core;

namespace BuildingSystem.Core.Interfaces
{
    public interface IBuildingRepository
    {
        void RegisterBuilding(GameObject building, BuildingData data, Vector3Int gridPosition);
        void UnregisterBuilding(GameObject building);
        bool TryGetBuildingInfo(GameObject building, out BuildingInfo info);
        IEnumerable<GameObject> GetAllBuildings();
        IEnumerable<GameObject> GetBuildingsOfType(string buildingId);
    }
}