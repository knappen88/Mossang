using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class HotbarSlotUI : MonoBehaviour,
    IDropHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectionOutline;
    [SerializeField] private TextMeshProUGUI keyText;

    private InventoryItem currentItem;
    private int slotIndex;

    private static HotbarSlotUI draggingSlot;

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }

    public void SetKeyNumber(string key)
    {
        if (keyText != null)
            keyText.text = key;
    }

    public void SetItem(InventoryItem item)
    {
        currentItem = item;

        if (item != null && item.itemData != null)
        {
            iconImage.sprite = item.itemData.icon;
            iconImage.enabled = true;
        }
        else
        {
            currentItem = null;
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    public InventoryItem GetItem() => currentItem;

    public void ClearSlot()
    {
        SetItem(null);
    }

    public void SetSelected(bool selected)
    {
        if (selectionOutline != null)
            selectionOutline.enabled = selected;
    }

    public void UseItem(GameObject user)
    {
        currentItem?.Use(user);
    }

    public static HotbarSlotUI GetDraggingSlot() => draggingSlot;

    // === DRAG & DROP ===

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        draggingSlot = this;
        DragDropManager.StartDrag(currentItem, this);

        // Визуальный эффект
        if (iconImage != null)
        {
            Color c = iconImage.color;
            c.a = 0.5f;
            iconImage.color = c;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Визуальное перетаскивание
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        draggingSlot = null;
        DragDropManager.EndDrag();

        // Возвращаем непрозрачность
        if (iconImage != null)
        {
            Color c = iconImage.color;
            c.a = 1f;
            iconImage.color = c;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedItem = DragDropManager.GetDraggedItem();
        if (draggedItem == null) return;

        var fromInventorySlot = DragDropManager.GetDragSource<InventorySlotUI>();
        var fromHotbarSlot = DragDropManager.GetDragSource<HotbarSlotUI>();

        // 1. Из инвентаря
        if (fromInventorySlot != null)
        {
            var inventoryUI = fromInventorySlot.GetInventoryUI();

            if (currentItem == null)
            {
                SetItem(draggedItem);
                inventoryUI.PlayerInventory.RemoveItem(draggedItem.itemData, draggedItem.quantity);
            }
            else
            {
                var tempItem = new InventoryItem(currentItem.itemData, currentItem.quantity);
                SetItem(draggedItem);
                inventoryUI.PlayerInventory.RemoveItem(draggedItem.itemData, draggedItem.quantity);
                inventoryUI.PlayerInventory.AddItem(tempItem);
            }
            return;
        }

        // 2. Из другого слота хотбара
        if (fromHotbarSlot != null && fromHotbarSlot != this)
        {
            var temp = currentItem;
            SetItem(draggedItem);
            fromHotbarSlot.SetItem(temp);
        }
    }
}