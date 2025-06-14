using UnityEngine;

[CreateAssetMenu(menuName = "Items/Resource", fileName = "New Resource")]
public class ResourceData : ItemData
{
    public override ItemType ItemType => ItemType.Resource;

    [Header("Resource Settings")]
    [SerializeField] private ResourceCategory category = ResourceCategory.Material;
    [SerializeField] private int sellPrice = 10;

    [Header("Visual")]
    [SerializeField] private GameObject dropPrefab; // Префаб для дропа (опционально)

    [Header("Crafting")]
    [SerializeField] private bool isCraftingMaterial = true;
    [SerializeField] private string description;

    public ResourceCategory Category => category;
    public int SellPrice => sellPrice;
    public bool IsCraftingMaterial => isCraftingMaterial;
    public string Description => description;

    public override void Use(GameObject user)
    {
        // Ресурсы обычно не используются напрямую
        Debug.Log($"This is a resource: {itemName}. It cannot be used directly.");
    }
}

public enum ResourceCategory
{
    Material,       // Дерево, камень, руда
    Food,          // Еда
    Plant,         // Растения, травы
    Liquid,        // Жидкости
    Gem,           // Драгоценные камни
    Misc           // Прочее
}