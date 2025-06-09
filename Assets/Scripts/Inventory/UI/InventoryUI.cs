using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private int slotCount = 42;

    private List<InventorySlotUI> slots = new();

    private void Awake()
    {
        // Создаём фиксированное количество слотов один раз
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, contentRoot);
            InventorySlotUI slot = slotGO.GetComponent<InventorySlotUI>();
            slots.Add(slot);
        }
    }

    private void OnEnable()
    {
        playerInventory.OnInventoryChanged.AddListener(RedrawUI);
        RedrawUI();
    }

    private void OnDisable()
    {
        playerInventory.OnInventoryChanged.RemoveListener(RedrawUI);
    }

    private void RedrawUI()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < playerInventory.items.Count)
            {
                var item = playerInventory.items[i];
                slots[i].SetItem(item.itemData, item.quantity);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
}
