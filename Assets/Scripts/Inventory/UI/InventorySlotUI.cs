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

        if (item != null && item.itemData != null)
        {
            itemIconImage.sprite = item.itemData.icon;
            itemIconImage.enabled = true;

            quantityText.text = item.itemData.isStackable && item.quantity > 1
                ? item.quantity.ToString()
                : "";
        }
        else
        {
            ClearSlot();
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
    public void SetItemDirectly(InventoryItem item) => linkedItem = item;

    // DRAG & DROP ======================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (linkedItem == null) return;

        draggingSlot = this;
        // TODO: DragIcon визуально (можем добавить позже)
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Пока ничего — только иконку можно двигать (если будет визуал)
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        draggingSlot = null;
    }
    public static InventorySlotUI GetDraggingSlot()
    {
        return draggingSlot;
    }


    public void OnDrop(PointerEventData eventData)
    {
        if (draggingSlot == null || draggingSlot == this) return;

        // Поменять местами предметы
        var temp = linkedItem;
        SetItem(draggingSlot.GetItem());
        draggingSlot.SetItem(temp);
    }
}
