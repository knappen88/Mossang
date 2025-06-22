using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Building System/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string id = System.Guid.NewGuid().ToString();
    [SerializeField] private string buildingName = "New Building";
    [SerializeField] private string description = "Building description";
    [SerializeField] private Sprite icon;

    [Header("Prefabs")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private GameObject demolishEffectPrefab;

    [Header("Size & Placement")]
    [SerializeField] private Vector2Int size = Vector2Int.one;
    [SerializeField] private bool canRotate = true;
    [SerializeField] private PlacementMode placementMode = PlacementMode.Grid;

    [Header("Requirements")]
    [SerializeField] private int requiredLevel = 1;
    [SerializeField] private ResourceRequirement[] resourceRequirements;
    [SerializeField] private string[] requiredQuestIds;
    [SerializeField] private string[] requiredBuildingIds;

    [Header("Properties")]
    [SerializeField] private BuildingCategory category = BuildingCategory.Production;
    [SerializeField] private float constructionTime = 5f;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool isDestructible = true;

    [Header("Production (if applicable)")]
    [SerializeField] private ResourceProduction[] resourceProduction;
    [SerializeField] private float productionInterval = 60f;

    [Header("Storage (if applicable)")]
    [SerializeField] private ResourceStorage[] resourceStorage;

    // Properties
    public string Id => id;
    public string BuildingName => buildingName;
    public string Description => description;
    public Sprite Icon => icon;

    public GameObject Prefab => prefab;
    public GameObject GhostPrefab => ghostPrefab ?? prefab;
    public GameObject DemolishEffectPrefab => demolishEffectPrefab;

    public Vector2Int Size => size;
    public bool CanRotate => canRotate;
    public PlacementMode PlacementMode => placementMode;

    public int RequiredLevel => requiredLevel;
    public ResourceRequirement[] ResourceRequirements => resourceRequirements;
    public string[] RequiredQuestIds => requiredQuestIds;
    public string[] RequiredBuildingIds => requiredBuildingIds;

    public BuildingCategory Category => category;
    public float ConstructionTime => constructionTime;
    public int MaxHealth => maxHealth;
    public bool IsDestructible => isDestructible;

    public ResourceProduction[] ResourceProduction => resourceProduction;
    public float ProductionInterval => productionInterval;
    public ResourceStorage[] ResourceStorage => resourceStorage;

    public bool CheckRequirements(int playerLevel, IEnumerable<string> completedQuests, IEnumerable<string> ownedBuildingIds)
    {
        if (playerLevel < requiredLevel) return false;

        foreach (var questId in requiredQuestIds)
        {
            if (!completedQuests.Contains(questId)) return false;
        }

        foreach (var buildingId in requiredBuildingIds)
        {
            if (!ownedBuildingIds.Contains(buildingId)) return false;
        }

        return true;
    }
}

public enum PlacementMode
{
    Grid,
    FreePlace,
    Road,
    Wall
}

public enum BuildingCategory
{
    Residential,
    Production,
    Storage,
    Military,
    Decoration,
    Infrastructure,
    Special
}

[System.Serializable]
public class ResourceRequirement
{
    public string resourceId;
    public int amount;

    public ResourceRequirement(string id, int amt)
    {
        resourceId = id;
        amount = amt;
    }
}

[System.Serializable]
public class ResourceProduction
{
    public string resourceId;
    public int amountPerCycle;
    public float efficiency = 1f;
}

[System.Serializable]
public class ResourceStorage
{
    public string resourceId;
    public int capacity;
}
}