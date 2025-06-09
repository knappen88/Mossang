using UnityEngine;
using UnityEngine.UI;

public class DragVisual : MonoBehaviour
{
    private static DragVisual instance;

    [SerializeField] private Image dragIcon;
    [SerializeField] private Canvas dragCanvas; // Canvas с высоким Sort Order

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void StartDrag(Sprite icon)
    {
        if (instance == null) return;

        instance.gameObject.SetActive(true);
        instance.dragIcon.sprite = icon;
        instance.dragIcon.SetNativeSize();
    }

    public static void EndDrag()
    {
        if (instance == null) return;

        instance.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            transform.position = Input.mousePosition;
        }
    }
}