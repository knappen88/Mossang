using UnityEngine;

[CreateAssetMenu(fileName = "GridConfig", menuName = "Building System/Grid Config")]
public class GridConfig : ScriptableObject
{
    [Header("Grid Settings")]
    [SerializeField] private Vector2 cellSize = Vector2.one;
    [SerializeField] private Vector3 cellGap = Vector3.zero;
    [SerializeField] private GridLayout.CellLayout cellLayout = GridLayout.CellLayout.Rectangle;
    [SerializeField] private GridLayout.CellSwizzle cellSwizzle = GridLayout.CellSwizzle.XYZ;

    [Header("Grid Bounds")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(100, 100);
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

    [Header("Terrain Validation")]
    [SerializeField] private bool validateTerrain = true;
    [SerializeField] private LayerMask terrainLayerMask = -1;
    [SerializeField] private float maxTerrainSlope = 30f;
    [SerializeField] private string[] allowedTerrainTags = { "Buildable" };

    // Properties
    public Vector2 CellSize => cellSize;
    public Vector3 CellGap => cellGap;
    public GridLayout.CellLayout CellLayout => cellLayout;
    public GridLayout.CellSwizzle CellSwizzle => cellSwizzle;

    public bool UseBounds => useBounds;
    public Vector2Int GridSize => gridSize;
    public Vector3 GridOrigin => gridOrigin;

    public bool ValidateTerrain => validateTerrain;
    public LayerMask TerrainLayerMask => terrainLayerMask;
    public float MaxTerrainSlope => maxTerrainSlope;
    public string[] AllowedTerrainTags => allowedTerrainTags;

    public Vector3Int GetGridBoundsMin()
    {
        return Vector3Int.FloorToInt(gridOrigin);
    }

    public Vector3Int GetGridBoundsMax()
    {
        return Vector3Int.FloorToInt(gridOrigin + new Vector3(gridSize.x, gridSize.y, 0));
    }

    public bool IsWithinBounds(Vector3Int gridPosition)
    {
        if (!useBounds) return true;

        var min = GetGridBoundsMin();
        var max = GetGridBoundsMax();

        return gridPosition.x >= min.x && gridPosition.x < max.x &&
               gridPosition.y >= min.y && gridPosition.y < max.y;
    }
}