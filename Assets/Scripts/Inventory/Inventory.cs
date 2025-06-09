using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public List<InventoryItem> items = new();

    public UnityEvent OnInventoryChanged;

    public void AddItem(ItemData item, int amount = 1)
    {
        if (item.isStackable)
        {
            var existing = items.Find(i => i.itemData == item);
            if (existing != null)
            {
                existing.quantity = Mathf.Min(existing.quantity + amount, item.maxStack);
            }
            else
            {
                items.Add(new InventoryItem(item, amount));
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
                items.Add(new InventoryItem(item, 1));
        }

        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(ItemData item, int amount = 1)
    {
        var existing = items.Find(i => i.itemData == item);
        if (existing != null)
        {
            existing.quantity -= amount;
            if (existing.quantity <= 0)
                items.Remove(existing);

            OnInventoryChanged?.Invoke();
        }
    }
}
