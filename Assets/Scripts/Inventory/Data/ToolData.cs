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
        // Инструменты тоже экипируются через тот же контроллер
        var equipmentController = user.GetComponent<Combat.Equipment.WeaponEquipmentController>();
        if (equipmentController != null)
        {
            // Временно конвертируем в WeaponData для совместимости
            // В будущем можно сделать базовый класс EquippableData
            var tempWeaponData = CreateWeaponDataFromTool();
            equipmentController.EquipWeapon(tempWeaponData);
            Debug.Log($"Экипирован инструмент: {itemName}");
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