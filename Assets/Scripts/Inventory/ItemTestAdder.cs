using UnityEngine;

public class ItemTestAdder : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemData itemToAdd;
    [SerializeField] private int quantity = 1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            inventory.AddItem(itemToAdd, quantity);
            Debug.Log($"[TestAdder] Добавлен предмет: {itemToAdd.name} x{quantity}");
        }
    }
}
