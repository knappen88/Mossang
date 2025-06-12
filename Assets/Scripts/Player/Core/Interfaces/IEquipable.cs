namespace Combat.Core
{
    using UnityEngine;

    public interface IEquippable
    {
        void OnEquip(GameObject owner);
        void OnUnequip();
        Transform GetEquipTransform();
    }
}