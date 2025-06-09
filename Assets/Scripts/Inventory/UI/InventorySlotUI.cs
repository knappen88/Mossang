using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

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
        }
        else
        {
            itemIconImage.sprite = null;
            itemIconImage.enabled = false;
            quantityText.text = "";
        }
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
            selectionHighlight.enabled = selected;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        inventoryUI.SelectSlot(this);

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
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Можно добавить визуальное перетаскивание
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        draggingSlot = null;
        DragDropManager.EndDrag();

        // Возвращаем непрозрачность
        if (itemIconImage != null)
        {
            Color c = itemIconImage.color;
            c.a = 1f;
            itemIconImage.color = c;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedItem = DragDropManager.GetDraggedItem();
        if (draggedItem == null) return;

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