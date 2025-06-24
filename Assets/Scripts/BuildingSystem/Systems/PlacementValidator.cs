using UnityEngine;
using BuildingSystem.Core.Interfaces;
using BuildingSystem.Core;

namespace BuildingSystem.Systems
{
    public class PlacementValidator : IPlacementValidator
    {
        private readonly IGridManager gridManager;
        private readonly GridConfig gridConfig;
        private readonly IResourceManager resourceManager;

        public PlacementValidator(IGridManager gridManager, GridConfig gridConfig, IResourceManager resourceManager)
        {
            this.gridManager = gridManager;
            this.gridConfig = gridConfig;
            this.resourceManager = resourceManager;
        }

        public PlacementValidationResult ValidatePlacement(Vector3Int gridPosition, BuildingData buildingData)
        {
            // Check bounds
            if (!CheckBounds(gridPosition, buildingData.Size))
            {
                return PlacementValidationResult.Invalid(
                    PlacementErrorType.OutOfBounds,
                    "Building is out of bounds");
            }

            // Check if cells are occupied
            if (!CheckCellsAvailable(gridPosition, buildingData.Size))
            {
                return PlacementValidationResult.Invalid(
                    PlacementErrorType.Occupied,
                    "Space is already occupied");
            }

            // Check terrain if enabled
            if (gridConfig.ValidateTerrain && !CheckTerrain(gridPosition, buildingData.Size))
            {
                return PlacementValidationResult.Invalid(
                    PlacementErrorType.InvalidTerrain,
                    "Invalid terrain for building");
            }

            // Check resources
            if (!resourceManager.HasResources(buildingData.ResourceRequirements))
            {
                return PlacementValidationResult.Invalid(
                    PlacementErrorType.InsufficientResources,
                    "Not enough resources");
            }

            // Check other requirements (level, quests, etc.)
            if (!CheckRequirements(buildingData))
            {
                return PlacementValidationResult.Invalid(
                    PlacementErrorType.RequirementsNotMet,
                    "Requirements not met");
            }

            return PlacementValidationResult.Valid();
        }

        private bool CheckBounds(Vector3Int gridPosition, Vector2Int size)
        {
            if (!gridConfig.UseBounds)
                return true;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var checkPos = new Vector3Int(gridPosition.x + x, gridPosition.y + y, 0);
                    if (!gridConfig.IsWithinBounds(checkPos))
                        return false;
                }
            }

            return true;
        }

        private bool CheckCellsAvailable(Vector3Int gridPosition, Vector2Int size)
        {
            return !gridManager.AreCellsOccupied(gridPosition, size);
        }

        private bool CheckTerrain(Vector3Int gridPosition, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var checkPos = new Vector3Int(gridPosition.x + x, gridPosition.y + y, 0);
                    var worldPos = gridManager.GridToWorld(checkPos);

                    // Raycast down to check terrain
                    if (Physics.Raycast(worldPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, gridConfig.TerrainLayerMask))
                    {
                        // Check slope
                        var angle = Vector3.Angle(Vector3.up, hit.normal);
                        if (angle > gridConfig.MaxTerrainSlope)
                            return false;

                        // Check terrain tag
                        bool validTag = false;
                        foreach (var tag in gridConfig.AllowedTerrainTags)
                        {
                            if (hit.collider.CompareTag(tag))
                            {
                                validTag = true;
                                break;
                            }
                        }

                        if (!validTag)
                            return false;
                    }
                    else
                    {
                        // No terrain found
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CheckRequirements(BuildingData buildingData)
        {
            // Get player data (you'll need to implement this based on your player system)
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return true; // Skip check if no player found

            // Check level requirement
            // var playerLevel = player.GetComponent<PlayerStats>()?.Level ?? 1;
            // if (playerLevel < buildingData.RequiredLevel)
            //     return false;

            // Check quest requirements
            // var questManager = player.GetComponent<QuestManager>();
            // if (questManager != null)
            // {
            //     foreach (var questId in buildingData.RequiredQuestIds)
            //     {
            //         if (!questManager.IsQuestCompleted(questId))
            //             return false;
            //     }
            // }

            // For now, return true since we don't have these systems implemented
            return true;
        }
    }
}