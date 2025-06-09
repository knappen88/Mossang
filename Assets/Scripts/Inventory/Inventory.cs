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

        Debug.Log($"[Inventory] AddItem: {itemData.itemName} x{quantity}");

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

        Debug.Log($"[Inventory] После добавления: {items.Count} предметов");
        OnInventoryChanged.Invoke();
    }

    public void AddItem(InventoryItem newItem)
    {
        if (newItem == null || newItem.itemData == null || newItem.quantity <= 0)
        {
            Debug.LogWarning("[Inventory] Попытка добавить null или пустой предмет");
            return;
        }

        AddItem(newItem.itemData, newItem.quantity);
    }

    public bool RemoveItem(ItemData itemData, int amount = 1)
    {
        Debug.Log($"[Inventory] RemoveItem: {itemData.itemName} x{amount}");

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemData == itemData)
            {
                items[i].quantity -= amount;

                if (items[i].quantity <= 0)
                {
                    items.RemoveAt(i);
                }

                Debug.Log($"[Inventory] После удаления: {items.Count} предметов");
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }
}