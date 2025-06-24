using BuildingSystem.Core.Events;
using BuildingSystem.Core.Interfaces;
using BuildingSystem.Core;
using UnityEngine;

public struct BuildingPlacementStartedEvent : IBuildingEvent
{
    public BuildingData BuildingData { get; }
    public BuildingPlacementStartedEvent(BuildingData data) => BuildingData = data;
}

public struct BuildingPlacedEvent : IBuildingEvent
{
    public GameObject Building { get; }
    public BuildingData Data { get; }
    public Vector3Int GridPosition { get; }

    public BuildingPlacedEvent(GameObject building, BuildingData data, Vector3Int gridPosition)
    {
        Building = building;
        Data = data;
        GridPosition = gridPosition;
    }
}

public struct BuildingPlacementCancelledEvent : IBuildingEvent
{
    public BuildingData BuildingData { get; }
    public BuildingPlacementCancelledEvent(BuildingData data) => BuildingData = data;
}

public struct BuildingConstructionStartedEvent : IBuildingEvent
{
    public GameObject Building { get; }
    public float Duration { get; }

    public BuildingConstructionStartedEvent(GameObject building, float duration)
    {
        Building = building;
        Duration = duration;
    }
}

public struct BuildingConstructionCompletedEvent : IBuildingEvent
{
    public GameObject Building { get; }
    public BuildingData Data { get; }

    public BuildingConstructionCompletedEvent(GameObject building, BuildingData data)
    {
        Building = building;
        Data = data;
    }
}

public struct BuildingDestroyedEvent : IBuildingEvent
{
    public GameObject Building { get; }
    public Vector3Int GridPosition { get; }

    public BuildingDestroyedEvent(GameObject building, Vector3Int gridPosition)
    {
        Building = building;
        GridPosition = gridPosition;
    }
}

public struct BuildingSelectedEvent : IBuildingEvent
{
    public GameObject Building { get; }
    public BuildingSelectedEvent(GameObject building) => Building = building;
}

public struct BuildingDeselectedEvent : IBuildingEvent
{
    public GameObject Building { get; }
    public BuildingDeselectedEvent(GameObject building) => Building = building;
}

public struct ResourcesChangedEvent : IBuildingEvent
{
    public string ResourceId { get; }
    public int OldAmount { get; }
    public int NewAmount { get; }

    public ResourcesChangedEvent(string resourceId, int oldAmount, int newAmount)
    {
        ResourceId = resourceId;
        OldAmount = oldAmount;
        NewAmount = newAmount;
    }
}

public struct PlacementValidationFailedEvent : IBuildingEvent
{
    public PlacementErrorType ErrorType { get; }
    public string Message { get; }

    public PlacementValidationFailedEvent(PlacementErrorType errorType, string message)
    {
        ErrorType = errorType;
        Message = message;
    }
}