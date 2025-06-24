using UnityEngine;
using System.Collections.Generic;
using BuildingSystem.Core.Interfaces;
using BuildingSystem.Config;

namespace BuildingSystem.Systems
{
    public class GridManager : IGridManager
    {
        private readonly Grid unityGrid;
        private readonly GridConfig config;
        private readonly Dictionary<Vector3Int, GameObject> occupiedCells = new Dictionary<Vector3Int, GameObject>();

        public GridManager(Grid unityGrid, GridConfig config)
        {
            this.unityGrid = unityGrid;
            this.config = config;
        }

        public Vector3Int WorldToGrid(Vector3 worldPosition)
        {
            return unityGrid.WorldToCell(worldPosition);
        }

        public Vector3 GridToWorld(Vector3Int gridPosition)
        {
            return unityGrid.GetCellCenterWorld(gridPosition);
        }

        public bool IsCellOccupied(Vector3Int gridPosition)
        {
            return occupiedCells.ContainsKey(gridPosition);
        }

        public bool AreCellsOccupied(Vector3Int startPosition, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var cellPos = new Vector3Int(startPosition.x + x, startPosition.y + y, 0);
                    if (IsCellOccupied(cellPos))
                        return true;
                }
            }
            return false;
        }

        public void OccupyCells(Vector3Int startPosition, Vector2Int size, GameObject building)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var cellPos = new Vector3Int(startPosition.x + x, startPosition.y + y, 0);
                    occupiedCells[cellPos] = building;
                }
            }
        }

        public void FreeCells(Vector3Int startPosition, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var cellPos = new Vector3Int(startPosition.x + x, startPosition.y + y, 0);
                    occupiedCells.Remove(cellPos);
                }
            }
        }

        public IEnumerable<Vector3Int> GetOccupiedCells()
        {
            return occupiedCells.Keys;
        }

        public GameObject GetBuildingAt(Vector3Int gridPosition)
        {
            return occupiedCells.TryGetValue(gridPosition, out var building) ? building : null;
        }
    }
}