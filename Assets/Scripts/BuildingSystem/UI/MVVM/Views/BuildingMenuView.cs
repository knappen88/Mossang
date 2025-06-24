using System.Collections.Generic;
using System.ComponentModel;
using BuildingSystem.Controllers;
using BuildingSystem.UI.MVVM.ViewModels;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuildingSystem.UI.MVVM.Views
{
    public class BuildingMenuView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelRect;

        [Header("Categories")]
        [SerializeField] private Transform categoryContainer;
        [SerializeField] private GameObject categoryButtonPrefab;

        [Header("Buildings")]
        [SerializeField] private Transform buildingContainer;
        [SerializeField] private GameObject buildingSlotPrefab;

        [Header("Selected Building Info")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TextMeshProUGUI buildingNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image buildingIcon;
        [SerializeField] private Transform requirementsContainer;
        [SerializeField] private GameObject requirementItemPrefab;

        [Header("Buttons")]
        [SerializeField] private Button selectButton;
        [SerializeField] private Button closeButton;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;

        private BuildingMenuViewModel viewModel;
        private Dictionary<BuildingCategory, Button> categoryButtons;
        private Dictionary<BuildingData, BuildingSlotView> buildingSlots;
        private List<GameObject> requirementItems;

        private void Awake()
        {
            categoryButtons = new Dictionary<BuildingCategory, Button>();
            buildingSlots = new Dictionary<BuildingData, BuildingSlotView>();
            requirementItems = new List<GameObject>();

            if (canvasGroup == null)
                canvasGroup = menuPanel.GetComponent<CanvasGroup>();

            closeButton.onClick.AddListener(() => viewModel?.CloseMenu());
            selectButton.onClick.AddListener(() => viewModel?.StartPlacement());

            menuPanel.SetActive(false);
        }

        public void Initialize(BuildingMenuViewModel viewModel)
        {
            // Unsubscribe from old view model
            if (this.viewModel != null)
            {
                this.viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                this.viewModel.OnBuildingStartPlacement -= OnBuildingStartPlacement;
            }

            this.viewModel = viewModel;

            // Subscribe to new view model
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModel.OnBuildingStartPlacement += OnBuildingStartPlacement;

            // Initial setup
            CreateCategoryButtons();
            UpdateView();
        }

        private void OnDestroy()
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                viewModel.OnBuildingStartPlacement -= OnBuildingStartPlacement;
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BuildingMenuViewModel.IsMenuOpen):
                    if (viewModel.IsMenuOpen)
                        ShowMenu();
                    else
                        HideMenu();
                    break;

                case nameof(BuildingMenuViewModel.SelectedCategory):
                    UpdateSelectedCategory();
                    break;

                case nameof(BuildingMenuViewModel.FilteredBuildings):
                    UpdateBuildingList();
                    break;

                case nameof(BuildingMenuViewModel.SelectedBuilding):
                    UpdateSelectedBuildingInfo();
                    break;

                case nameof(BuildingMenuViewModel.CanAffordSelected):
                    UpdateSelectButton();
                    break;
            }
        }

        private void CreateCategoryButtons()
        {
            // Clear existing buttons
            foreach (var kvp in categoryButtons)
            {
                Destroy(kvp.Value.gameObject);
            }
            categoryButtons.Clear();

            // Create buttons for each category
            foreach (var category in viewModel.Categories)
            {
                var buttonGO = Instantiate(categoryButtonPrefab, categoryContainer);
                var button = buttonGO.GetComponent<Button>();
                var text = buttonGO.GetComponentInChildren<TextMeshProUGUI>();

                if (text != null)
                    text.text = GetCategoryDisplayName(category);

                button.onClick.AddListener(() => viewModel.SelectCategory(category));
                categoryButtons[category] = button;
            }
        }

        private void UpdateView()
        {
            UpdateSelectedCategory();
            UpdateBuildingList();
            UpdateSelectedBuildingInfo();
            UpdateSelectButton();
        }

        private void UpdateSelectedCategory()
        {
            foreach (var kvp in categoryButtons)
            {
                var isSelected = kvp.Key == viewModel.SelectedCategory;
                kvp.Value.interactable = !isSelected;
            }
        }

        private void UpdateBuildingList()
        {
            // Clear existing building slots
            foreach (var kvp in buildingSlots)
            {
                Destroy(kvp.Value.gameObject);
            }
            buildingSlots.Clear();

            // Create slots for filtered buildings
            foreach (var building in viewModel.FilteredBuildings)
            {
                var slotGO = Instantiate(buildingSlotPrefab, buildingContainer);
                var slot = slotGO.GetComponent<BuildingSlotView>();
                
                if (slot == null)
                    slot = slotGO.AddComponent<BuildingSlotView>();

                slot.Initialize(building, () => viewModel.SelectBuilding(building));
                buildingSlots[building] = slot;
            }
        }

        private void UpdateSelectedBuildingInfo()
        {
            var building = viewModel.SelectedBuilding;
            if (building == null)
            {
                infoPanel.SetActive(false);
                return;
            }

            infoPanel.SetActive(true);

            if (buildingNameText != null)
                buildingNameText.text = building.BuildingName;

            if (descriptionText != null)
                descriptionText.text = building.Description;

            if (buildingIcon != null && building.Icon != null)
                buildingIcon.sprite = building.Icon;

            UpdateRequirements();
        }

        private void UpdateRequirements()
        {
            // Clear existing requirement items
            foreach (var item in requirementItems)
            {
                Destroy(item);
            }
            requirementItems.Clear();

            if (viewModel.SelectedBuilding == null || requirementsContainer == null)
                return;

            // Create requirement items
            foreach (var req in viewModel.SelectedBuildingRequirements)
            {
                var itemGO = Instantiate(requirementItemPrefab, requirementsContainer);
                var texts = itemGO.GetComponentsInChildren<TextMeshProUGUI>();

                if (texts.Length >= 2)
                {
                    texts[0].text = req.ResourceId;
                    texts[1].text = $"{req.Available}/{req.Required}";
                    texts[1].color = req.HasEnough ? Color.green : Color.red;
                }

                requirementItems.Add(itemGO);
            }
        }

        private void UpdateSelectButton()
        {
            selectButton.interactable = viewModel.CanAffordSelected;
        }

        public void Show()
        {
            viewModel?.OpenMenu();
        }

        private void ShowMenu()
        {
            menuPanel.SetActive(true);

            // Animate
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, animationDuration);

            panelRect.localScale = Vector3.one * 0.8f;
            panelRect.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack);
        }

        private void HideMenu()
        {
            canvasGroup.DOFade(0f, animationDuration);
            panelRect.DOScale(Vector3.one * 0.8f, animationDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => menuPanel.SetActive(false));
        }

        private void OnBuildingStartPlacement(BuildingData building)
        {
            var controller = FindObjectOfType<BuildingSystemController>();
            controller?.StartPlacement(building);
        }

        private string GetCategoryDisplayName(BuildingCategory category)
        {
            return category switch
            {
                BuildingCategory.Residential => "Жилые",
                BuildingCategory.Production => "Производство",
                BuildingCategory.Storage => "Хранилища",
                BuildingCategory.Military => "Военные",
                BuildingCategory.Decoration => "Декорации",
                BuildingCategory.Infrastructure => "Инфраструктура",
                BuildingCategory.Special => "Особые",
                _ => category.ToString()
            };
        }
    }
}