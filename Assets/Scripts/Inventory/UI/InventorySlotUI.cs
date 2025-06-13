using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

public class InventorySlotUI : MonoBehaviour,
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image selectionHighlight;

    private InventoryUI inventoryUI;
    private InventoryItem linkedItem;

    private float lastClickTime;
    private const float doubleClickThreshold = 0.25f;

    private static InventorySlotUI draggingSlot;

    public void Init(InventoryUI ui)
    {
        inventoryUI = ui;
        SetSelected(false);
    }

    public void SetItem(InventoryItem item)
    {
        linkedItem = item;

        if (linkedItem != null && linkedItem.itemData != null)
        {
            itemIconImage.sprite = linkedItem.itemData.icon;
            itemIconImage.enabled = true;

            quantityText.text = linkedItem.itemData.isStackable && linkedItem.quantity > 1
                ? linkedItem.quantity.ToString()
                : "";

            // Анимация появления предмета
            AnimateItemAppear();
        }
        else
        {
            itemIconImage.sprite = null;
            itemIconImage.enabled = false;
            quantityText.text = "";
        }
    }

    private void AnimateItemAppear()
    {
        if (itemIconImage == null) return;

        itemIconImage.transform.localScale = Vector3.zero;
        itemIconImage.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void ClearSlot()
    {
        linkedItem = null;
        itemIconImage.sprite = null;
        itemIconImage.enabled = false;
        quantityText.text = "";
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.enabled = selected;

            // Анимация выделения
            if (selected)
            {
                selectionHighlight.transform.localScale = Vector3.one * 0.9f;
                selectionHighlight.transform.DOScale(1f, 0.2f).SetEase(Ease.OutElastic);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        inventoryUI.SelectSlot(this);

        // Визуальный эффект клика
        transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5);

        // Двойной клик — переместить в хотбар
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            inventoryUI.TryMoveToHotbar(this);
        }

        lastClickTime = Time.time;
    }

    public InventoryItem GetItem() => linkedItem;
    public static InventorySlotUI GetDraggingSlot() => draggingSlot;
    public InventoryUI GetInventoryUI() => inventoryUI;

    // DRAG & DROP ======================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (linkedItem == null) return;

        draggingSlot = this;
        DragDropManager.StartDrag(linkedItem, this);

        // Визуальный эффект
        if (itemIconImage != null)
        {
            Color c = itemIconImage.color;
            c.a = 0.5f;
            itemIconImage.color = c;

            // Используем DragVisual
            DragVisual.StartDrag(itemIconImage.sprite);
        }

        // Уменьшаем слот
        transform.DOScale(0.9f, 0.1f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // DragVisual обрабатывает визуальное перетаскивание
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        draggingSlot = null;
        DragDropManager.EndDrag();
        DragVisual.EndDrag();

        // Возвращаем визуальное состояние
        if (itemIconImage != null)
        {
            Color c = itemIconImage.color;
            c.a = 1f;
            itemIconImage.color = c;
        }

        // Возвращаем размер
        transform.DOScale(1f, 0.1f);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedItem = DragDropManager.GetDraggedItem();
        if (draggedItem == null) return;

        // Анимация приема предмета
        transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 5);

        // Проверяем источник перетаскивания
        var fromInventorySlot = DragDropManager.GetDragSource<InventorySlotUI>();
        var fromHotbarSlot = DragDropManager.GetDragSource<HotbarSlotUI>();

        // 1. Из другого слота инвентаря
        if (fromInventorySlot != null && fromInventorySlot != this)
        {
            var tempItem = linkedItem;
            SetItem(draggedItem);
            fromInventorySlot.SetItem(tempItem);
            inventoryUI.UpdateInventoryFromSlots();
            return;
        }

        // 2. Из хотбара
        if (fromHotbarSlot != null)
        {
            Debug.Log($"[OnDrop] Из хотбара: {draggedItem.itemData.itemName} x{draggedItem.quantity}");

            if (linkedItem == null)
            {
                // Добавляем в инвентарь
                inventoryUI.PlayerInventory.AddItem(draggedItem);
                // Очищаем хотбар
                fromHotbarSlot.ClearSlot();
            }
            else
            {
                // Меняем местами
                var tempItem = new InventoryItem(linkedItem.itemData, linkedItem.quantity);

                inventoryUI.PlayerInventory.RemoveItem(linkedItem.itemData, linkedItem.quantity);
                inventoryUI.PlayerInventory.AddItem(draggedItem);

                fromHotbarSlot.SetItem(tempItem);
            }
        }
    }
}