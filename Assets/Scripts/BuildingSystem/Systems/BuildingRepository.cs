using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BuildingSystem.Core.Interfaces;
using BuildingSystem.Core;

namespace BuildingSystem.Systems
{
    public class BuildingRepository : IBuildingRepository
    {
        private readonly Dictionary<GameObject, BuildingInfo> buildings = new Dictionary<GameObject, BuildingInfo>();
        private readonly Dictionary<string, List<GameObject>> buildingsByType = new Dictionary<string, List<GameObject>>();

        public void RegisterBuilding(GameObject building, BuildingData data, Vector3Int gridPosition)
        {
            if (buildings.ContainsKey(building))
            {
                Debug.LogWarning($"Building {building.name} is already registered!");
                return;
            }

            var info = new BuildingInfo
            {
                Data = data,
                GridPosition = gridPosition,
                State = BuildingState.UnderConstruction
            };

            buildings[building] = info;

            // Add to type dictionary
            if (!buildingsByType.ContainsKey(data.Id))
            {
                buildingsByType[data.Id] = new List<GameObject>();
            }
            buildingsByType[data.Id].Add(building);
        }

        public void UnregisterBuilding(GameObject building)
        {
            if (!buildings.TryGetValue(building, out var info))
                return;

            buildings.Remove(building);

            // Remove from type dictionary
            if (buildingsByType.TryGetValue(info.Data.Id, out var list))
            {
                list.Remove(building);
                if (list.Count == 0)
                {
                    buildingsByType.Remove(info.Data.Id);
                }
            }
        }

        public bool TryGetBuildingInfo(GameObject building, out BuildingInfo info)
        {
            return buildings.TryGetValue(building, out info);
        }

        public IEnumerable<GameObject> GetAllBuildings()
        {
            return buildings.Keys;
        }

        public IEnumerable<GameObject> GetBuildingsOfType(string buildingId)
        {
            if (buildingsByType.TryGetValue(buildingId, out var list))
            {
                return list;
            }
            return Enumerable.Empty<GameObject>();
        }

        public void UpdateBuildingState(GameObject building, BuildingState newState)
        {
            if (buildings.TryGetValue(building, out var info))
            {
                info.State = newState;
                buildings[building] = info;
            }
        }

        public int GetBuildingCount(string buildingId)
        {
            if (buildingsByType.TryGetValue(buildingId, out var list))
            {
                return list.Count;
            }
            return 0;
        }

        public IEnumerable<string> GetOwnedBuildingIds()
        {
            return buildingsByType.Keys;
        }
    }
}