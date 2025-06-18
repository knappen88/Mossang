using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

namespace Building.UI
{
    public class BuildingMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Transform categoriesContainer;
        [SerializeField] private Transform buildingsContainer;
        [SerializeField] private GameObject categoryButtonPrefab;
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
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Building Database")]
        [SerializeField] private List<BuildingData> allBuildings = new List<BuildingData>();

        [Header("Panel Animation")]
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private float showY = 10f;
        [SerializeField] private float hideY = -250f;
        [SerializeField] private float animationSpeed = 0.3f;

        private BuildingSystem buildingSystem;
        private Inventory playerInventory;
        private Dictionary<BuildingCategory, List<BuildingData>> categorizedBuildings;
        private BuildingData selectedBuilding;
        private int playerLevel = 1;

        private void Awake()
        {
            buildingSystem = FindObjectOfType<BuildingSystem>();

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                playerInventory = player.GetComponent<Inventory>();

            if (canvasGroup == null)
                canvasGroup = menuPanel.GetComponent<CanvasGroup>();

            CategorizeBuildings();

            CreateCategoryButtons();

            if (selectButton != null)
                selectButton.onClick.AddListener(OnSelectBuilding);

            if (closeButton != null)
                closeButton.onClick.AddListener(CloseMenu);

            menuPanel.SetActive(false);
        }

        private void CategorizeBuildings()
        {
            categorizedBuildings = new Dictionary<BuildingCategory, List<BuildingData>>();

            foreach (BuildingCategory category in System.Enum.GetValues(typeof(BuildingCategory)))
            {
                categorizedBuildings[category] = new List<BuildingData>();
            }

            foreach (var building in allBuildings)
            {
                if (building != null)
                    categorizedBuildings[building.category].Add(building);
            }
        }

        private void CreateCategoryButtons()
        {
            foreach (Transform child in categoriesContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (BuildingCategory category in System.Enum.GetValues(typeof(BuildingCategory)))
            {
                if (categorizedBuildings[category].Count == 0)
                    continue;

                GameObject btnGO = Instantiate(categoryButtonPrefab, categoriesContainer);
                Button btn = btnGO.GetComponent<Button>();
                TextMeshProUGUI text = btnGO.GetComponentInChildren<TextMeshProUGUI>();

                if (text != null)
                    text.text = GetCategoryName(category);

                BuildingCategory cat = category;
                btn.onClick.AddListener(() => ShowBuildingsInCategory(cat));
            }

            ShowBuildingsInCategory(BuildingCategory.Storage);
        }

        private void ShowBuildingsInCategory(BuildingCategory category)
        {
            foreach (Transform child in buildingsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var building in categorizedBuildings[category])
            {
                CreateBuildingSlot(building);
            }
        }

        private void CreateBuildingSlot(BuildingData building)
        {
            GameObject slotGO = Instantiate(buildingSlotPrefab, buildingsContainer);
            BuildingSlotUI slot = slotGO.GetComponent<BuildingSlotUI>();

            if (slot == null)
            {
                Button btn = slotGO.GetComponent<Button>();
                Image icon = slotGO.transform.Find("Icon")?.GetComponent<Image>();
                TextMeshProUGUI nameText = slotGO.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                GameObject lockOverlay = slotGO.transform.Find("LockOverlay")?.gameObject;

                if (icon != null && building.icon != null)
                    icon.sprite = building.icon;

                if (nameText != null)
                    nameText.text = building.buildingName;

                bool isUnlocked = building.IsUnlocked(playerLevel);
                if (lockOverlay != null)
                    lockOverlay.SetActive(!isUnlocked);

                btn.interactable = isUnlocked;
                btn.onClick.AddListener(() => SelectBuilding(building));
            }
            else
            {
                slot.Setup(building, playerLevel);
                slot.OnSlotClicked += SelectBuilding;
            }
        }

        private void SelectBuilding(BuildingData building)
        {
            selectedBuilding = building;
            ShowBuildingInfo(building);

            bool canAfford = building.CanAfford(playerInventory);
            selectButton.interactable = canAfford;

            infoPanel.transform.DOPunchScale(Vector3.one * 0.05f, 0.2f);
        }

        private void ShowBuildingInfo(BuildingData building)
        {
            infoPanel.SetActive(true);

            buildingNameText.text = building.buildingName;
            descriptionText.text = building.description;
            buildingIcon.sprite = building.icon;

            foreach (Transform child in requirementsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var requirement in building.resourceRequirements)
            {
                GameObject reqGO = Instantiate(requirementItemPrefab, requirementsContainer);
                Image icon = reqGO.transform.Find("Icon")?.GetComponent<Image>();
                TextMeshProUGUI text = reqGO.transform.Find("Amount")?.GetComponent<TextMeshProUGUI>();

                if (icon != null && requirement.resource.icon != null)
                    icon.sprite = requirement.resource.icon;

                if (text != null)
                {
                    int playerAmount = GetPlayerResourceAmount(requirement.resource);
                    text.text = $"{playerAmount}/{requirement.amount}";

                    text.color = playerAmount >= requirement.amount ? Color.green : Color.red;
                }
            }
        }

        private int GetPlayerResourceAmount(ItemData resource)
        {
            if (playerInventory == null) return 0;

            int count = 0;
            foreach (var item in playerInventory.items)
            {
                if (item.itemData == resource)
                    count += item.quantity;
            }
            return count;
        }

        private void OnSelectBuilding()
        {
            if (selectedBuilding == null || buildingSystem == null)
                return;

            buildingSystem.StartPlacement(selectedBuilding);

            CloseMenu();
        }

        public void ToggleMenu()
        {
            if (menuPanel.activeSelf)
                CloseMenu();
            else
                OpenMenu();
        }

        public void OpenMenu()
        {
            menuPanel.SetActive(true);

            panelRect.DOAnchorPosY(showY, animationSpeed)
            .SetEase(Ease.OutQuad);

            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, animationDuration);

            menuPanel.transform.localScale = Vector3.one * 0.8f;
            menuPanel.transform.DOScale(Vector3.one, animationDuration)
                .SetEase(Ease.OutBack);

            var playerMovement = playerInventory?.GetComponent<PlayerMovement>();
            if (playerMovement != null)
                playerMovement.DisableMovement();
        }

        public void CloseMenu()
        {
            canvasGroup.DOFade(0f, animationDuration);

            panelRect.DOAnchorPosY(hideY, animationSpeed)
        .SetEase(Ease.InQuad)
        .OnComplete(() => menuPanel.SetActive(false));

            menuPanel.transform.DOScale(Vector3.one * 0.8f, animationDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    menuPanel.SetActive(false);

                    var playerMovement = playerInventory?.GetComponent<PlayerMovement>();
                    if (playerMovement != null)
                        playerMovement.EnableMovement();
                });
        }

        private string GetCategoryName(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Storage: return "Хранилища";
                case BuildingCategory.Production: return "Производство";
                case BuildingCategory.Crafting: return "Ремесло";
                case BuildingCategory.Decoration: return "Декорации";
                case BuildingCategory.Defense: return "Защита";
                case BuildingCategory.Special: return "Особые";
                default: return category.ToString();
            }
        }
    }
}