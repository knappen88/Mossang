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

    private void Awake()
    {
        Debug.Log($"[HotbarManager] Awake - GameObject '{gameObject.name}' активен: {gameObject.activeSelf}");
        Debug.Log($"[HotbarManager] Родитель: {transform.parent?.name ?? "нет"}");

        // Проверяем, что сам HotbarManager активен
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError($"[HotbarManager] GameObject неактивен в иерархии!");
        }
    }

    private void Start()
    {
        Debug.Log($"[HotbarManager] Start - Создаю {slotCount} слотов");

        // Проверяем slotParent
        if (slotParent == null)
        {
            Debug.LogError("[HotbarManager] slotParent не назначен!");
            slotParent = transform;
        }

        // Создаем слоты
        for (int i = 0; i < slotCount; i++)
        {
            if (slotPrefab == null)
            {
                Debug.LogError("[HotbarManager] slotPrefab не назначен!");
                return;
            }

            var slotGO = Instantiate(slotPrefab, slotParent);
            var slotUI = slotGO.GetComponent<HotbarSlotUI>();

            if (slotUI == null)
            {
                Debug.LogError($"[HotbarManager] HotbarSlotUI не найден на префабе!");
                continue;
            }

            slotUI.SetSlotIndex(i);
            slotUI.SetKeyNumber((i + 1).ToString());
            slotUI.SetSelected(i == activeSlotIndex);
            slots.Add(slotUI);

            Debug.Log($"[HotbarManager] Создан слот {i}");
        }

        Debug.Log($"[HotbarManager] Создано {slots.Count} слотов. Хотбар активен: {gameObject.activeInHierarchy}");
    }

    private void OnEnable()
    {
        Debug.Log($"[HotbarManager] OnEnable - хотбар включен");
    }

    private void OnDisable()
    {
        Debug.LogWarning($"[HotbarManager] OnDisable - хотбар ВЫКЛЮЧЕН!");
    }

    public bool TryAddToHotbar(InventoryItem item)
    {
        foreach (var slot in slots)
        {
            if (slot.GetItem() == null)
            {
                slot.SetItem(item.Clone());

                // Если добавляем в активный слот, сразу активируем предмет
                if (slots.IndexOf(slot) == activeSlotIndex)
                {
                    slot.UseItem(player);
                }

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

                // Если в слоте есть предмет - используем его
                if (slots[i].GetItem() != null)
                {
                    slots[i].UseItem(player);
                }
                // Если слот пустой - автоматически активируются кулаки через UnequipItem
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

        // Если слот пустой, экипируем кулаки
        if (slots[index].GetItem() == null)
        {
            var equipmentController = player.GetComponent<Player.Equipment.EquipmentController>();
            if (equipmentController != null)
            {
                equipmentController.UnequipItem(); // Это автоматически экипирует unarmed
            }
        }
    }

    public List<HotbarSlotUI> GetSlots() => slots;

    // Метод для уведомления о том, что в слот был добавлен предмет
    public void OnItemAddedToSlot(int slotIndex)
    {
        if (slotIndex == activeSlotIndex && slots[slotIndex].GetItem() != null)
        {
            // Если предмет добавлен в активный слот, активируем его
            slots[slotIndex].UseItem(player);
        }
    }
}