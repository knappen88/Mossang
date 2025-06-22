using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace BuildingSystem.Systems
{
    public class BuildingRepository : IBuildingRepository
{
    private readonly Dictionary<Guid, BuildingInfo> buildings = new Dictionary<Guid, BuildingInfo>();
    private readonly Dictionary<GameObject, Guid> gameObjectToId = new Dictionary<GameObject, Guid>();

    public void RegisterBuilding(GameObject building, BuildingData data, Vector3Int gridPosition)
    {
        var id = Guid.NewGuid();
        var info = new BuildingInfo
        {
            Id = id,
            Data = data,
            GridPosition = gridPosition,
            State = BuildingState.UnderConstruction,
            ConstructionProgress = 0f
        };

        buildings[id] = info;
        gameObjectToId[building] = id;
    }

    public void UnregisterBuilding(GameObject building)
    {
        if (gameObjectToId.TryGetValue(building, out var id))
        {
            buildings.Remove(id);
            gameObjectToId.Remove(building);
        }
    }

    public GameObject GetBuilding(Guid id)
    {
        var kvp = gameObjectToId.FirstOrDefault(x => x.Value == id);
        return kvp.Key;
    }

    public IEnumerable<GameObject> GetAllBuildings()
    {
        return gameObjectToId.Keys;
    }

    public IEnumerable<GameObject> GetBuildingsByCategory(BuildingCategory category)
    {
        return gameObjectToId
            .Where(kvp => buildings[kvp.Value].Data.Category == category)
            .Select(kvp => kvp.Key);
    }

    public BuildingData GetBuildingData(GameObject building)
    {
        if (gameObjectToId.TryGetValue(building, out var id) && buildings.TryGetValue(id, out var info))
        {
            return info.Data;
        }
        return null;
    }

    public bool TryGetBuildingInfo(GameObject building, out BuildingInfo info)
    {
        if (gameObjectToId.TryGetValue(building, out var id))
        {
            return buildings.TryGetValue(id, out info);
        }
        info = default;
        return false;
    }

    public void UpdateBuildingState(GameObject building, BuildingState newState)
    {
        if (gameObjectToId.TryGetValue(building, out var id) && buildings.TryGetValue(id, out var info))
        {
            info.State = newState;
            buildings[id] = info;
        }
    }

    public void UpdateConstructionProgress(GameObject building, float progress)
    {
        if (gameObjectToId.TryGetValue(building, out var id) && buildings.TryGetValue(id, out var info))
        {
            info.ConstructionProgress = Mathf.Clamp01(progress);
            buildings[id] = info;
        }
    }
}
}