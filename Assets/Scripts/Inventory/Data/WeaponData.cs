using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon")]
public class WeaponData : ItemData
{
    public override ItemType ItemType => ItemType.Weapon;

    public float damage;
    public float attackSpeed;
    public AnimationClip attackAnimation;

    public override void Use(GameObject user)
    {
        Debug.Log($"Атака оружием {itemName}. Урон: {damage}");
        // Запуск анимации и логики атаки через контроллер игрока
    }
}
