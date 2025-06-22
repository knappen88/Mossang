using System.Collections.Generic;
using System.ComponentModel;
using BuildingSystem.Controllers;
using BuildingSystem.UI.MVVM.ViewModels;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
                UpdateSelectedBuilding();
                break;

            case nameof(BuildingMenuViewModel.CanAffordSelected):
                UpdateSelectButton();
                break;

            case nameof(BuildingMenuViewModel.SelectedBuildingRequirements):
                UpdateRequirements();
                break;
        }
    }

    private void CreateCategoryButtons()
    {
        // Clear existing
        foreach (Transform child in categoryContainer)
        {
            Destroy(child.gameObject);
        }
        categoryButtons.Clear();

        // Create new buttons
        foreach (var category in viewModel.Categories)
        {
            var buttonGO = Instantiate(categoryButtonPrefab, categoryContainer);
            var button = buttonGO.GetComponent<Button>();
            var text = buttonGO.GetComponentInChildren<TextMeshProUGUI>();

            if (text != null)
                text.text = GetCategoryDisplayName(category);

            var cat = category; // Capture for closure
            button.onClick.AddListener(() => viewModel.SelectCategory(cat));

            categoryButtons[category] = button;
        }
    }

    private void UpdateSelectedCategory()
    {
        foreach (var kvp in categoryButtons)
        {
            var isSelected = kvp.Key == viewModel.SelectedCategory;
            // Update visual state
            var colors = kvp.Value.colors;
            colors.normalColor = isSelected ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            kvp.Value.colors = colors;
        }
    }

    private void UpdateBuildingList()
    {
        // Clear existing slots
        foreach (Transform child in buildingContainer)
        {
            Destroy(child.gameObject);
        }
        buildingSlots.Clear();

        // Create new slots
        foreach (var building in viewModel.FilteredBuildings)
        {
            var slotGO = Instantiate(buildingSlotPrefab, buildingContainer);
            var slotView = slotGO.GetComponent<BuildingSlotView>();

            if (slotView != null)
            {
                slotView.Initialize(building, () => viewModel.SelectBuilding(building));
                buildingSlots[building] = slotView;
            }
        }
    }

    private void UpdateSelectedBuilding()
    {
        if (viewModel.SelectedBuilding == null)
        {
            infoPanel.SetActive(false);
            return;
        }

        infoPanel.SetActive(true);

        var building = viewModel.SelectedBuilding;
        buildingNameText.text = building.BuildingName;
        descriptionText.text = building.Description;
        buildingIcon.sprite = building.Icon;

        UpdateRequirements();
        UpdateSelectButton();

        // Animate info panel
        infoPanel.transform.DOKill();
        infoPanel.transform.DOPunchScale(Vector3.one * 0.05f, 0.2f);
    }

    private void UpdateRequirements()
    {
        // Clear existing
        foreach (var item in requirementItems)
        {
            Destroy(item);
        }
        requirementItems.Clear();

        // Create new requirement displays
        foreach (var req in viewModel.SelectedBuildingRequirements)
        {
            var itemGO = Instantiate(requirementItemPrefab, requirementsContainer);
            var icon = itemGO.transform.Find("Icon")?.GetComponent<Image>();
            var text = itemGO.transform.Find("Amount")?.GetComponent<TextMeshProUGUI>();

            if (text != null)
            {
                text.text = $"{req.Available}/{req.Required}";
                text.color = req.HasEnough ? Color.green : Color.red;
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