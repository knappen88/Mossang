using System.Collections.Generic;
using UnityEngine;

public class HotbarManager : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private int slotCount = 4;
    [SerializeField] private GameObject player;

    private List<HotbarSlotUI> slots = new();
    private int activeSlotIndex = 0;

    private void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            var slotGO = Instantiate(slotPrefab, slotParent);
            var slotUI = slotGO.GetComponent<HotbarSlotUI>();
            slotUI.SetSlotIndex(i);
            slotUI.SetKeyNumber((i + 1).ToString());
            slotUI.SetSelected(i == activeSlotIndex); // ← выделяем первый
            slots.Add(slotUI);
        }
    }
    public bool TryAddToHotbar(InventoryItem item)
    {
        foreach (var slot in slots)
        {
            if (slot.GetItem() == null)
            {
                slot.SetItem(item);
                return true;
            }
        }

        return false;
    }

    private void Update()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
                slots[i].UseItem(player);
            }
        }
    }

    private void SelectSlot(int index)
    {
        activeSlotIndex = index;

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetSelected(i == index);
        }
    }

    public List<HotbarSlotUI> GetSlots() => slots;
}
