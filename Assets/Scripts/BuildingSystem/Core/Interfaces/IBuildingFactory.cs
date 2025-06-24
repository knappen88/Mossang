using UnityEngine;

namespace BuildingSystem.Core.Interfaces
{
    public interface IBuildingFactory
    {
        GameObject CreateBuilding(BuildingData data, Vector3 position, Quaternion rotation);
        GameObject CreateGhost(BuildingData data);
        void RecycleBuilding(GameObject building);
        void RecycleGhost(GameObject ghost);
    }
}