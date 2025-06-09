using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public List<InventoryItem> items = new();

    public UnityEvent OnInventoryChanged;
    public int maxSlots = 36;


    public void AddItem(ItemData itemData, int quantity)
    {
        if (itemData == null || quantity <= 0) return;

        if (itemData.isStackable)
        {
            foreach (var item in items)
            {
                if (item.itemData == itemData && item.quantity < itemData.maxStack)
                {
                    int space = itemData.maxStack - item.quantity;
                    int toAdd = Mathf.Min(space, quantity);
                    item.quantity += toAdd;
                    quantity -= toAdd;

                    if (quantity <= 0)
                    {
                        OnInventoryChanged.Invoke();
                        return;
                    }
                }
            }
        }

        while (quantity > 0)
        {
            if (items.Count >= maxSlots)
            {
                Debug.Log("Инвентарь переполнен, не удалось добавить остаток");
                break;
            }

            int toAdd = itemData.isStackable ? Mathf.Min(itemData.maxStack, quantity) : 1;

            items.Add(new InventoryItem(itemData, toAdd));
            quantity -= toAdd;
        }

        OnInventoryChanged.Invoke();
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
