using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI quantityText;

    public void SetItem(ItemData item, int amount)
    {
        itemIconImage.sprite = item.icon;
        itemIconImage.enabled = true;

        quantityText.text = item.isStackable && amount > 1 ? amount.ToString() : "";
    }

    public void ClearSlot()
    {
        itemIconImage.sprite = null;
        itemIconImage.enabled = false;
        quantityText.text = "";
    }
}
