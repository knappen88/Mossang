using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class HotbarSlotUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image iconImage;               // Иконка предмета
    [SerializeField] private Image selectionOutline;        // Рамка выделения
    [SerializeField] private TextMeshProUGUI keyText;       // Номер клавиши

    private int slotIndex;
    private InventoryItem currentItem;

    public void SetSlotIndex(int index) => slotIndex = index;

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
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    public InventoryItem GetItem() => currentItem;

    public void UseItem(GameObject user)
    {
        currentItem?.Use(user);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = InventorySlotUI.GetDraggingSlot();
        if (dragged == null) return;

        SetItem(dragged.GetItem());
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionOutline != null)
            selectionOutline.enabled = isSelected;
    }
}
