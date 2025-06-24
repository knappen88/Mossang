using UnityEngine;

namespace BuildingSystem.Core
{
    public struct BuildingInfo
    {
        public BuildingData Data { get; set; }
        public Vector3Int GridPosition { get; set; }
        public BuildingState State { get; set; }
    }

    public enum BuildingState
    {
        UnderConstruction,
        Completed,
        Damaged,
        Destroyed
    }

    public struct PlacementValidationResult
    {
        public bool IsValid { get; set; }
        public PlacementErrorType ErrorType { get; set; }
        public string ErrorMessage { get; set; }

        public static PlacementValidationResult Valid()
        {
            return new PlacementValidationResult
            {
                IsValid = true,
                ErrorType = PlacementErrorType.None,
                ErrorMessage = string.Empty
            };
        }

        public static PlacementValidationResult Invalid(PlacementErrorType errorType, string message)
        {
            return new PlacementValidationResult
            {
                IsValid = false,
                ErrorType = errorType,
                ErrorMessage = message
            };
        }
    }

    public enum PlacementErrorType
    {
        None,
        OutOfBounds,
        Occupied,
        InvalidTerrain,
        InsufficientResources,
        RequirementsNotMet,
        TooFarFromPlayer
    }
}