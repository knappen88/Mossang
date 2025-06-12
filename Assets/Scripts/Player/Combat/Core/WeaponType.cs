namespace Combat.Core
{
    /// <summary>
    /// Типы оружия в игре
    /// </summary>
    public enum WeaponType
    {
        // Оружие ближнего боя
        Sword,
        Axe,
        Hammer,
        Dagger,
        Spear,

        // Инструменты
        Pickaxe,
        Shovel,
        Hoe,
        FishingRod,

        // Оружие дальнего боя
        Bow,
        Crossbow,
        ThrowingKnife,

        // Магическое оружие
        Staff,
        Wand,
        Orb,

        // Особое
        Shield,
        Torch,
        None
    }
}