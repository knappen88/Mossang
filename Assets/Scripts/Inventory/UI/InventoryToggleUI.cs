using UnityEngine;

public class InventoryToggleUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    private bool isOpen = false;

    private void Start()
    {
        inventoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            isOpen = !isOpen;
            inventoryPanel.SetActive(isOpen);
        }
    }
}
