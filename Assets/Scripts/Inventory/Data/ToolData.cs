using UnityEngine;
using Combat.Data;

[CreateAssetMenu(menuName = "Items/Tool")]
public class ToolData : ItemData
{
    public override ItemType ItemType => ItemType.Tool;

    [Header("Tool Settings")]
    public ToolType toolType;
    public float useDuration;
    public float efficiency = 1f; // Множитель эффективности

    [Header("Tool Visuals")]
    public GameObject toolPrefab; // 3D модель инструмента
    public WeaponAnimationSet animationSet; // Анимации для инструмента

    [Header("Resource Gathering")]
    public ResourceType[] gatherableResources; // Какие ресурсы может добывать
    public float gatherRadius = 2f;
    public int damagePerUse = 10; // Урон ресурсному объекту за использование

    [Header("Audio")]
    public AudioClip[] useSounds;
    public AudioClip[] hitSounds;

    public override void Use(GameObject user)
    {
        // Используем тот же EquipmentController что и для оружия
        var equipmentController = user.GetComponent<Player.Equipment.EquipmentController>();
        if (equipmentController != null)
        {
            equipmentController.EquipItem(this);
        }
        else
        {
            Debug.LogWarning($"EquipmentController not found on {user.name}");
        }
    }
    private WeaponData CreateWeaponDataFromTool()
    {
        // Создаем временный WeaponData для инструмента
        var weaponData = ScriptableObject.CreateInstance<WeaponData>();
        weaponData.itemName = itemName;
        weaponData.icon = icon;
        weaponData.weaponPrefab = toolPrefab;
        weaponData.animationSet = animationSet;
        weaponData.damage = damagePerUse;
        return weaponData;
    }
}

public enum ToolType
{
    Axe,
    Pickaxe,
    Shovel,
    FishingRod,
    Hammer
}

public enum ResourceType
{
    Wood,
    Stone,
    Ore,
    Plants,
    Fish
}