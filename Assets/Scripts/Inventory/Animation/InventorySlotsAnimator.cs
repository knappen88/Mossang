using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class InventorySlotsAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float slotDelay = 0.02f;
    [SerializeField] private float slotDuration = 0.2f;
    [SerializeField] private Ease slotEase = Ease.OutBack;

    [Header("References")]
    [SerializeField] private Transform inventorySlotsContainer;

    private InventoryToggleUI toggleUI;
    private InventorySlotUI[] inventorySlots;
    private Transform hotbarTransform; // Кэшируем transform хотбара

    private void Awake()
    {
        toggleUI = GetComponent<InventoryToggleUI>();

        // Кэшируем transform хотбара один раз
        GameObject hotbarObj = GameObject.Find("Hotbar");
        if (hotbarObj != null)
        {
            hotbarTransform = hotbarObj.transform;
        }

        // Получаем слоты
        CollectInventorySlots();
    }

    private void CollectInventorySlots()
    {
        if (inventorySlotsContainer != null)
        {
            inventorySlots = inventorySlotsContainer.GetComponentsInChildren<InventorySlotUI>(true);
        }
        else
        {
            // Собираем только слоты инвентаря, исключая хотбар
            var allSlots = GetComponentsInChildren<InventorySlotUI>(true);
            var inventorySlotsList = new List<InventorySlotUI>();

            foreach (var slot in allSlots)
            {
                // Проверяем что слот не из хотбара
                if (!IsSlotFromHotbar(slot))
                {
                    inventorySlotsList.Add(slot);
                }
            }

            inventorySlots = inventorySlotsList.ToArray();
        }

        Debug.Log($"[InventorySlotsAnimator] Found {inventorySlots.Length} inventory slots");
    }

    private bool IsSlotFromHotbar(InventorySlotUI slot)
    {
        // Проверяем по компоненту HotbarSlotUI
        if (slot.GetComponentInParent<HotbarSlotUI>() != null)
            return true;

        // Проверяем по иерархии если есть кэшированный hotbar
        if (hotbarTransform != null && slot.transform.IsChildOf(hotbarTransform))
            return true;

        // Проверяем по имени родителя
        Transform parent = slot.transform.parent;
        while (parent != null)
        {
            if (parent.name.ToLower().Contains("hotbar"))
                return true;
            parent = parent.parent;
        }

        return false;
    }

    private void OnEnable()
    {
        if (inventorySlots != null && inventorySlots.Length > 0)
        {
            StartCoroutine(AnimateSlots());
        }
    }

    private void OnDisable()
    {
        // Останавливаем все анимации при отключении
        StopAllCoroutines();
    }

    private IEnumerator AnimateSlots()
    {
        // Ждем один кадр чтобы UI обновился
        yield return null;

        // Анимируем только слоты инвентаря
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            var slot = inventorySlots[i];
            if (slot == null) continue;

            var rectTransform = slot.GetComponent<RectTransform>();
            if (rectTransform == null) continue;

            // Начальное состояние
            rectTransform.localScale = Vector3.zero;

            // Анимация
            rectTransform.DOScale(1f, slotDuration)
                .SetEase(slotEase)
                .SetDelay(i * slotDelay);
        }
    }

    private void OnDestroy()
    {
        // Останавливаем все твины при уничтожении
        foreach (var slot in inventorySlots)
        {
            if (slot != null)
            {
                var rectTransform = slot.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.DOKill();
                }
            }
        }
    }
}