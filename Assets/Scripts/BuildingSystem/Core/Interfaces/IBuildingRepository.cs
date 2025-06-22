using System.Collections.Generic;
using UnityEngine;


namespace BuildingSystem.Core.Interfaces
{
    public interface IBuildingRepository
    {
        void RegisterBuilding(GameObject building, BuildingData data, Vector3Int gridPosition);
        void UnregisterBuilding(GameObject building);
        GameObject GetBuilding(System.Guid id);
        IEnumerable<GameObject> GetAllBuildings();
        IEnumerable<GameObject> GetBuildingsByCategory(BuildingCategory category);
        BuildingData GetBuildingData(GameObject building);
        bool TryGetBuildingInfo(GameObject building, out BuildingInfo info);
    }

    public struct BuildingInfo
    {
        public System.Guid Id { get; set; }
        public BuildingData Data { get; set; }
        public Vector3Int GridPosition { get; set; }
        public float ConstructionProgress { get; set; }
        public BuildingState State { get; set; }
    }

    public enum BuildingState
    {
        Ghost,
        UnderConstruction,
        Completed,
        Damaged,
        Destroyed
    }
}