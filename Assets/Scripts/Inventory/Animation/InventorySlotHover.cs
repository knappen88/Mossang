using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(InventorySlotUI))]
public class InventorySlotHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float hoverDuration = 0.1f;

    private RectTransform rectTransform;
    private Tween currentTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        currentTween?.Kill();
        currentTween = rectTransform.DOScale(hoverScale, hoverDuration)
            .SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        currentTween?.Kill();
        currentTween = rectTransform.DOScale(1f, hoverDuration)
            .SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
    }
}