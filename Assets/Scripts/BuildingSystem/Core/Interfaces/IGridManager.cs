using UnityEngine;
using System.Collections.Generic;

namespace BuildingSystem.Core.Interfaces
{
    public interface IGridManager
    {
        Vector3Int WorldToGrid(Vector3 worldPosition);
        Vector3 GridToWorld(Vector3Int gridPosition);
        bool IsCellOccupied(Vector3Int gridPosition);
        bool AreCellsOccupied(Vector3Int startPosition, Vector2Int size);
        void OccupyCells(Vector3Int startPosition, Vector2Int size, GameObject building);
        void FreeCells(Vector3Int startPosition, Vector2Int size);
        IEnumerable<Vector3Int> GetOccupiedCells();
        GameObject GetBuildingAt(Vector3Int gridPosition);
    }
}