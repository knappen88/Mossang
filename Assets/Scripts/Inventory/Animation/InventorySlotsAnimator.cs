using UnityEngine;
using DG.Tweening;
using System.Collections;

public class InventorySlotsAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float slotDelay = 0.02f;
    [SerializeField] private float slotDuration = 0.2f;
    [SerializeField] private Ease slotEase = Ease.OutBack;

    [Header("References")]
    [SerializeField] private Transform inventorySlotsContainer; // Контейнер со слотами инвентаря

    private InventoryToggleUI toggleUI;
    private InventorySlotUI[] inventorySlots;

    private void Awake()
    {
        toggleUI = GetComponent<InventoryToggleUI>();

        // Получаем слоты только из контейнера инвентаря, а не из всего UI
        if (inventorySlotsContainer != null)
        {
            inventorySlots = inventorySlotsContainer.GetComponentsInChildren<InventorySlotUI>(true);
        }
        else
        {
            // Если контейнер не указан, пытаемся найти слоты по имени родителя
            var allSlots = GetComponentsInChildren<InventorySlotUI>(true);
            var tempList = new System.Collections.Generic.List<InventorySlotUI>();

            foreach (var slot in allSlots)
            {
                // Исключаем слоты хотбара
                if (!slot.GetComponentInParent<HotbarSlotUI>() &&
                    !slot.transform.IsChildOf(GameObject.Find("Hotbar")?.transform))
                {
                    tempList.Add(slot);
                }
            }

            inventorySlots = tempList.ToArray();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(AnimateSlots());
    }

    private IEnumerator AnimateSlots()
    {
        // Ждем один кадр чтобы UI обновился
        yield return null;

        // Анимируем только слоты инвентаря, не трогая хотбар
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            var slot = inventorySlots[i];
            if (slot == null) continue;

            var rectTransform = slot.GetComponent<RectTransform>();

            // Начальное состояние
            rectTransform.localScale = Vector3.zero;

            // Анимация
            rectTransform.DOScale(1f, slotDuration)
                .SetEase(slotEase)
                .SetDelay(i * slotDelay);
        }
    }
}