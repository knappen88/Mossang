using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Building.UI
{
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

        public System.Action<BuildingData> OnSlotClicked;

        private void Awake()
        {
            button = GetComponent<Button>();
            originalScale = transform.localScale;

            if (iconImage == null)
                iconImage = transform.Find("Icon")?.GetComponent<Image>();

            if (nameText == null)
                nameText = transform.Find("Name")?.GetComponent<TextMeshProUGUI>();

            if (lockOverlay == null)
                lockOverlay = transform.Find("LockOverlay")?.gameObject;
        }

        public void Setup(BuildingData data, int playerLevel)
        {
            buildingData = data;

            if (iconImage != null && data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.color = normalColor;
            }

            if (nameText != null)
            {
                nameText.text = data.buildingName;
            }

            isUnlocked = data.IsUnlocked(playerLevel);
            UpdateLockState(playerLevel);

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

            if (iconImage != null)
            {
                iconImage.color = isUnlocked ? normalColor : lockedColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isUnlocked) return;

            transform.DOScale(originalScale * hoverScale, animationDuration)
                .SetEase(Ease.OutQuad);

            if (iconImage != null)
            {
                iconImage.DOColor(Color.white * 1.2f, animationDuration);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isUnlocked) return;

            transform.DOScale(originalScale, animationDuration)
                .SetEase(Ease.OutQuad);

            if (iconImage != null)
            {
                iconImage.DOColor(normalColor, animationDuration);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isUnlocked)
            {
                transform.DOShakePosition(0.5f, 10f, 10);
                return;
            }

            transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5);

            OnSlotClicked?.Invoke(buildingData);
        }

        public void UpdateState(int playerLevel, Inventory playerInventory)
        {
            bool wasUnlocked = isUnlocked;
            isUnlocked = buildingData.IsUnlocked(playerLevel);

            if (wasUnlocked != isUnlocked)
            {
                UpdateLockState(playerLevel);

                if (isUnlocked)
                {
                    PlayUnlockAnimation();
                }
            }

            if (isUnlocked && playerInventory != null)
            {
                bool canAfford = buildingData.CanAfford(playerInventory);

                if (nameText != null)
                {
                    nameText.color = canAfford ? Color.white : Color.yellow;
                }
            }
        }

        private void PlayUnlockAnimation()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(originalScale, 0.5f)
                .SetEase(Ease.OutBounce);

            if (iconImage != null)
            {
                iconImage.DOColor(Color.white * 2f, 0.2f)
                    .OnComplete(() => iconImage.DOColor(normalColor, 0.3f));
            }
        }
    }
}