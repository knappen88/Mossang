using UnityEngine;
using System.Collections.Generic;

namespace BuildingSystem.Core.Interfaces
{
    public interface IPlacementValidator
    {
        bool CanPlace(Vector3Int gridPosition, BuildingData buildingData);
        PlacementValidationResult ValidatePlacement(Vector3Int gridPosition, BuildingData buildingData);
    }
    public struct PlacementValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public PlacementErrorType ErrorType { get; set; }
    }

    public enum PlacementErrorType
    {
        None,
        OutOfBounds,
        Occupied,
        InvalidTerrain,
        InsufficientResources,
        RequirementsNotMet
    }
}