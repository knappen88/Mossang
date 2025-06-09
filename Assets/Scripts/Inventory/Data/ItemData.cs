using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isStackable = true;
    public int maxStack = 99;

    public abstract ItemType ItemType { get; }

    // Базовый метод — переопределяется в наследниках
    public virtual void Use(GameObject user)
    {
        Debug.Log($"Использован предмет: {itemName}");
    }
}
