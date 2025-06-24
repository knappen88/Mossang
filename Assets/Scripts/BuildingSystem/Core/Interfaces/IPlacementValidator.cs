using UnityEngine;
using BuildingSystem.Core;

namespace BuildingSystem.Core.Interfaces
{
    public interface IPlacementValidator
    {
        PlacementValidationResult ValidatePlacement(Vector3Int gridPosition, BuildingData buildingData);
    }
}