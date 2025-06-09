using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image selectionHighlight;

    private InventoryUI inventoryUI;
    private InventoryItem linkedItem;

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
    }

    public InventoryItem GetItem() => linkedItem;
}
