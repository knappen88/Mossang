using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

namespace Building
{
    /// <summary>
    /// Компонент для отображения призрака здания и требуемых ресурсов
    /// </summary>
    public class BuildingGhost : MonoBehaviour
    {
        private BuildingData buildingData;
        private GameObject resourceDisplay;
        private List<ResourceDisplayItem> resourceItems = new List<ResourceDisplayItem>();
        private Canvas worldCanvas;

        public void Initialize(BuildingData data, GameObject resourceDisplayPrefab)
        {
            buildingData = data;

            // Создаем Canvas для отображения ресурсов
            CreateWorldCanvas();

            // Создаем отображение ресурсов
            if (resourceDisplayPrefab != null)
            {
                resourceDisplay = Instantiate(resourceDisplayPrefab, worldCanvas.transform);
                CreateResourceDisplay();
            }
        }

        private void CreateWorldCanvas()
        {
            GameObject canvasGO = new GameObject("Resource Canvas");
            canvasGO.transform.SetParent(transform);
            canvasGO.transform.localPosition = Vector3.zero;

            worldCanvas = canvasGO.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.sortingLayerName = "UI";
            worldCanvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100;

            // Позиционируем Canvas над зданием
            RectTransform rt = canvasGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 100);

            // Рассчитываем позицию на основе размера здания
            float yOffset = (buildingData.size.y * 0.5f) + 1f;
            canvasGO.transform.localPosition = new Vector3(0, yOffset, 0);
        }

        private void CreateResourceDisplay()
        {
            if (resourceDisplay == null) return;

            // Создаем панель для ресурсов
            GameObject panel = new GameObject("Resource Panel");
            panel.transform.SetParent(resourceDisplay.transform);

            RectTransform panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchoredPosition = Vector2.zero;
            panelRT.sizeDelta = new Vector2(180, 40);

            // Фон панели
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

            // Горизонтальный layout
            HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Создаем элементы для каждого ресурса
            foreach (var requirement in buildingData.resourceRequirements)
            {
                CreateResourceItem(panel.transform, requirement);
            }

            // Анимация появления
            panel.transform.localScale = Vector3.zero;
            panel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        private void CreateResourceItem(Transform parent, ResourceRequirement requirement)
        {
            GameObject itemGO = new GameObject($"Resource_{requirement.resource.itemName}");
            itemGO.transform.SetParent(parent);

            RectTransform rt = itemGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50, 30);

            // Иконка ресурса
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(itemGO.transform);

            RectTransform iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.anchoredPosition = new Vector2(-10, 0);
            iconRT.sizeDelta = new Vector2(24, 24);

            Image icon = iconGO.AddComponent<Image>();
            icon.sprite = requirement.resource.icon;

            // Текст количества
            GameObject textGO = new GameObject("Amount");
            textGO.transform.SetParent(itemGO.transform);

            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchoredPosition = new Vector2(12, 0);
            textRT.sizeDelta = new Vector2(30, 30);

            TextMeshProUGUI amountText = textGO.AddComponent<TextMeshProUGUI>();
            amountText.text = requirement.amount.ToString();
            amountText.fontSize = 16;
            amountText.fontStyle = FontStyles.Bold;
            amountText.alignment = TextAlignmentOptions.Left;

            // Сохраняем для обновления
            resourceItems.Add(new ResourceDisplayItem
            {
                requirement = requirement,
                amountText = amountText,
                icon = icon
            });
        }

        public void UpdateResourceDisplay(bool canPlace)
        {
            foreach (var item in resourceItems)
            {
                // Цветовая индикация
                item.amountText.color = canPlace ? Color.green : Color.red;

                // Пульсация если нельзя разместить
                if (!canPlace)
                {
                    item.amountText.transform.DOKill();
                    item.amountText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 5);
                }
            }
        }

        private void Update()
        {
            // Поворачиваем Canvas к камере
            if (worldCanvas != null && Camera.main != null)
            {
                worldCanvas.transform.rotation = Camera.main.transform.rotation;
            }
        }

        private void OnDestroy()
        {
            // Очистка
            if (resourceDisplay != null)
                Destroy(resourceDisplay);
        }

        private class ResourceDisplayItem
        {
            public ResourceRequirement requirement;
            public TextMeshProUGUI amountText;
            public Image icon;
        }
    }
}