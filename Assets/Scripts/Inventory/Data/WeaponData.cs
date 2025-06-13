using UnityEngine;
using Combat.Data;

[CreateAssetMenu(menuName = "Items/Weapon")]
public class WeaponData : ItemData
{
    public override ItemType ItemType => ItemType.Weapon;

    [Header("Combat Stats")]
    public float damage;
    public float attackSpeed;
    public float attackRange = 1.5f;
    public float knockbackForce = 2f;

    [Header("Weapon Visuals")]
    public GameObject weaponPrefab; // 3D модель оружия
    public WeaponAnimationSet animationSet; // Набор анимаций

    [Header("Combat Settings")]
    public LayerMask targetLayers = -1; // На какие слои действует оружие
    public bool canBlock = false;
    public float blockDamageReduction = 0.5f;

    [Header("Audio")]
    public AudioClip[] swingSounds;
    public AudioClip[] hitSounds;

    public override void Use(GameObject user)
    {
        // Ищем EquipmentController на игроке
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
}