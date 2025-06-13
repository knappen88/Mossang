using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace UI
{
    /// <summary>
    /// UI подсказка для подбора предметов
    /// </summary>
    public class PickupHintUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI actionText;
        [SerializeField] private Image itemIcon;

        [Header("Settings")]
        [SerializeField] private string actionKey = "E";
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.15f;

        private static PickupHintUI instance;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;

        private void Awake()
        {
            instance = this;

            canvasGroup = hintPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = hintPanel.AddComponent<CanvasGroup>();
            }

            rectTransform = hintPanel.GetComponent<RectTransform>();

            // Начальное состояние
            canvasGroup.alpha = 0;
            hintPanel.SetActive(false);

            // Устанавливаем текст действия
            if (actionText != null)
            {
                actionText.text = $"Press [{actionKey}] to pickup";
            }
        }

        public static void Show(ItemData item, int quantity = 1)
        {
            if (instance == null) return;
            instance.ShowHint(item, quantity);
        }

        public static void Hide()
        {
            if (instance == null) return;
            instance.HideHint();
        }

        private void ShowHint(ItemData item, int quantity)
        {
            if (item == null) return;

            hintPanel.SetActive(true);

            // Устанавливаем данные
            if (itemNameText != null)
            {
                string quantityText = quantity > 1 ? $" x{quantity}" : "";
                itemNameText.text = item.itemName + quantityText;
            }

            if (itemIcon != null && item.icon != null)
            {
                itemIcon.sprite = item.icon;
                itemIcon.enabled = true;
            }

            // Анимация появления
            canvasGroup.DOKill();
            canvasGroup.DOFade(1f, fadeInDuration);

            // Небольшая анимация масштаба
            rectTransform.localScale = Vector3.one * 0.8f;
            rectTransform.DOScale(1f, fadeInDuration).SetEase(Ease.OutBack);
        }

        private void HideHint()
        {
            // Анимация исчезновения
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, fadeOutDuration)
                .OnComplete(() => hintPanel.SetActive(false));

            rectTransform.DOScale(0.8f, fadeOutDuration).SetEase(Ease.InBack);
        }
    }
}