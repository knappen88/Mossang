using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BuildingSystem.UI.MVVM.Views
{
    [RequireComponent(typeof(Button))]
    public class BuildingSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private CanvasGroup canvasGroup;

        private BuildingData buildingData;
        private Button button;
        private System.Action onClickCallback;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Initialize(BuildingData data, System.Action onClick)
        {
            buildingData = data;
            onClickCallback = onClick;

            // Set icon and name
            if (iconImage != null && data.Icon != null)
                iconImage.sprite = data.Icon;

            if (nameText != null)
                nameText.text = data.BuildingName;

            // Setup button
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickCallback?.Invoke());

            // Check if locked (simplified - you'd check actual requirements)
            var isLocked = false; // Check player level, quests, etc.
            if (lockOverlay != null)
                lockOverlay.SetActive(isLocked);

            button.interactable = !isLocked;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOScale(1.05f, 0.2f);
            if (canvasGroup != null)
                canvasGroup.DOFade(0.8f, 0.2f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(1f, 0.2f);
            if (canvasGroup != null)
                canvasGroup.DOFade(1f, 0.2f);
        }
    }
}