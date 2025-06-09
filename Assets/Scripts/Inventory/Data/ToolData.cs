using UnityEngine;

[CreateAssetMenu(menuName = "Items/Tool")]
public class ToolData : ItemData
{
    public override ItemType ItemType => ItemType.Tool;

    public AnimationClip useAnimation;
    public float useDuration;

    public override void Use(GameObject user)
    {
        Debug.Log($"Использован инструмент {itemName}");
        // Логика рубки дерева, сбора и т.д.
    }
}
