using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Building.UI
{
    /// <summary>
    /// UI компонент для отображения здания в меню выбора
    /// </summary>
    public class BuildingSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private Image lockIcon;
        [SerializeField] private TextMeshProUGUI lockReasonText;

        [Header("Visual Settings")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float animationDuration = 0.2f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private BuildingData buildingData;
        private Button button;
        private bool isUnlocked = false;
        private Vector3 originalScale;

        // События
        public System.Action<BuildingData> OnSlotClicked;

        private void Awake()
        {
            button = GetComponent<Button>();
            originalScale = transform.localScale;

            // Находим элементы если не назначены
            if (iconImage == null)
                iconImage = transform.Find("Icon")?.GetComponent<Image>();

            if (nameText == null)
                nameText = transform.Find("Name")?.GetComponent<TextMeshProUGUI>();

            if (lockOverlay == null)
                lockOverlay = transform.Find("LockOverlay")?.gameObject;
        }

        /// <summary>
        /// Настройка слота с данными здания
        /// </summary>
        public void Setup(BuildingData data, int playerLevel)
        {
            buildingData = data;

            // Устанавливаем иконку
            if (iconImage != null && data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.color = normalColor;
            }

            // Устанавливаем название
            if (nameText != null)
            {
                nameText.text = data.buildingName;
            }

            // Проверяем доступность
            isUnlocked = data.IsUnlocked(playerLevel);
            UpdateLockState(playerLevel);

            // Настраиваем кнопку
            if (button != null)
            {
                button.interactable = isUnlocked;
            }
        }

        private void UpdateLockState(int playerLevel)
        {
            if (lockOverlay != null)
            {
                lockOverlay.SetActive(!isUnlocked);

                // Показываем причину блокировки
                if (!isUnlocked && lockReasonText != null)
                {
                    if (playerLevel < buildingData.requiredLevel)
                    {
                        lockReasonText.text = $"Уровень {buildingData.requiredLevel}";
                    }
                    else if (buildingData.requiredQuests.Count > 0)
                    {
                        lockReasonText.text = "Требуется квест";
                    }
                }
            }

            // Затемняем иконку если заблокировано
            if (iconImage != null)
            {
                iconImage.color = isUnlocked ? normalColor : lockedColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isUnlocked) return;

            // Анимация увеличения
            transform.DOScale(originalScale * hoverScale, animationDuration)
                .SetEase(Ease.OutQuad);

            // Подсветка
            if (iconImage != null)
            {
                iconImage.DOColor(Color.white * 1.2f, animationDuration);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isUnlocked) return;

            // Возврат к исходному размеру
            transform.DOScale(originalScale, animationDuration)
                .SetEase(Ease.OutQuad);

            // Убираем подсветку
            if (iconImage != null)
            {
                iconImage.DOColor(normalColor, animationDuration);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isUnlocked)
            {
                // Эффект тряски для заблокированного слота
                transform.DOShakePosition(0.5f, 10f, 10);
                return;
            }

            // Эффект клика
            transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5);

            // Вызываем событие
            OnSlotClicked?.Invoke(buildingData);
        }

        /// <summary>
        /// Обновить состояние слота
        /// </summary>
        public void UpdateState(int playerLevel, Inventory playerInventory)
        {
            // Обновляем состояние блокировки
            bool wasUnlocked = isUnlocked;
            isUnlocked = buildingData.IsUnlocked(playerLevel);

            if (wasUnlocked != isUnlocked)
            {
                UpdateLockState(playerLevel);

                // Анимация разблокировки
                if (isUnlocked)
                {
                    PlayUnlockAnimation();
                }
            }

            // Проверяем доступность ресурсов
            if (isUnlocked && playerInventory != null)
            {
                bool canAfford = buildingData.CanAfford(playerInventory);

                // Визуальная индикация доступности ресурсов
                if (nameText != null)
                {
                    nameText.color = canAfford ? Color.white : Color.yellow;
                }
            }
        }

        private void PlayUnlockAnimation()
        {
            // Эффект разблокировки
            transform.localScale = Vector3.zero;
            transform.DOScale(originalScale, 0.5f)
                .SetEase(Ease.OutBounce);

            // Вспышка
            if (iconImage != null)
            {
                iconImage.DOColor(Color.white * 2f, 0.2f)
                    .OnComplete(() => iconImage.DOColor(normalColor, 0.3f));
            }
        }
    }
}