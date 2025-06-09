using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private int slotCount = 42;
    [SerializeField] private List<HotbarSlotUI> hotbarSlots;
    [SerializeField] private HotbarManager hotbarManager;

    private List<InventorySlotUI> slots = new();
    private InventorySlotUI selectedSlot;

    public Inventory PlayerInventory => playerInventory;

    private void Awake()
    {
        // Создаём фиксированное количество слотов один раз
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, contentRoot);
            InventorySlotUI slot = slotGO.GetComponent<InventorySlotUI>();

            slot.Init(this);
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
                slots[i].SetItem(item);
            }
            else
            {
                slots[i].ClearSlot();
            }

            slots[i].SetSelected(false);
        }

        selectedSlot = null;
    }

    // Новый метод для обновления инвентаря из слотов UI
    public void UpdateInventoryFromSlots()
    {
        // Собираем все непустые предметы из слотов
        var newItems = new List<InventoryItem>();

        foreach (var slot in slots)
        {
            var item = slot.GetItem();
            if (item != null && item.itemData != null)
            {
                newItems.Add(item);
            }
        }

        // Обновляем инвентарь
        playerInventory.items = newItems;
        playerInventory.OnInventoryChanged.Invoke();
    }

    public void SelectSlot(InventorySlotUI slot)
    {
        if (selectedSlot != null)
            selectedSlot.SetSelected(false);

        selectedSlot = slot;

        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(true);
            var item = selectedSlot.GetItem();
            Debug.Log("Выбран слот с предметом: " + item?.itemData?.itemName);
        }
    }

    public void TryMoveToHotbar(InventorySlotUI slot)
    {
        var item = slot.GetItem();
        if (item == null) return;

        bool added = hotbarManager.TryAddToHotbar(item);

        if (added)
        {
            playerInventory.RemoveItem(item.itemData, item.quantity);
            RedrawUI();
        }
        else
        {
            Debug.Log("Нет свободного слота в хотбаре");
        }
    }

    public void ForceRedraw()
    {
        // Принудительное обновление всех слотов
        StartCoroutine(ForceRedrawCoroutine());
    }

    private System.Collections.IEnumerator ForceRedrawCoroutine()
    {
        // Ждем конец кадра
        yield return new WaitForEndOfFrame();

        // Перерисовываем UI
        RedrawUI();
    }
}