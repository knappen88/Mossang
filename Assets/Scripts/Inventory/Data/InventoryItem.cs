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
        itemData.Use(user);
    }
}
