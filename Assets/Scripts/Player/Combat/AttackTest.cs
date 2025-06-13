
using UnityEngine;
using Player.Equipment;

public class AttackTest : MonoBehaviour
{
    private EquipmentController equipmentController;

    private void Start()
    {
        equipmentController = GetComponent<EquipmentController>();
    }

    private void Update()
    {
        // Атака на ЛКМ
        if (Input.GetMouseButtonDown(0))
        {
            if (equipmentController != null && equipmentController.HasItemEquipped())
            {
                equipmentController.Attack();
            }
            else
            {
                Debug.Log("No weapon equipped!");
            }
        }
    }
}