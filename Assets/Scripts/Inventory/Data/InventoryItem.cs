using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public int quantity;

    public InventoryItem(ItemData data, int amount)
    {
        itemData = data;
        quantity = amount;
    }

    public void Use(GameObject user)
    {
        if (itemData != null)
            itemData.Use(user);
    }

    public InventoryItem Clone()
    {
        return new InventoryItem(itemData, quantity);
    }
}