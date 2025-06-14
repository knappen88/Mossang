using UnityEngine;

public class DragDropManager : MonoBehaviour
{
    private static DragDropManager instance;

    private InventoryItem draggedItem;
    private object dragSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Только делаем DontDestroyOnLoad если объект в корне иерархии
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Очищаем статическую ссылку при уничтожении
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void StartDrag(InventoryItem item, object source)
    {
        if (instance == null || item == null) return;

        // Создаем копию предмета для безопасности
        instance.draggedItem = new InventoryItem(item.itemData, item.quantity);
        instance.dragSource = source;

        Debug.Log($"[DragDropManager] Начато перетаскивание: {item.itemData.itemName} x{item.quantity}");
    }

    public static void EndDrag()
    {
        if (instance == null) return;

        instance.draggedItem = null;
        instance.dragSource = null;
    }

    public static InventoryItem GetDraggedItem()
    {
        return instance?.draggedItem;
    }

    public static T GetDragSource<T>() where T : class
    {
        return instance?.dragSource as T;
    }

    public static bool IsDragging()
    {
        return instance != null && instance.draggedItem != null;
    }
}