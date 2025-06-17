using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

namespace Building
{
    /// <summary>
    /// Компонент строительной площадки - здание в процессе постройки
    /// </summary>
    public class ConstructionSite : MonoBehaviour
    {
        [Header("Visual")]
        private SpriteRenderer[] spriteRenderers;
        private Color constructionColor = new Color(1f, 1f, 1f, 0.5f);
        private ParticleSystem constructionParticles;

        [Header("UI")]
        private GameObject progressBarPrefab;
        private Slider progressBar;
        private TextMeshProUGUI infoText;
        private Canvas worldCanvas;

        private BuildingData buildingData;
        private Inventory playerInventory;
        private bool isConstructing = false;
        private bool resourcesDelivered = false;
        private float constructionProgress = 0f;

        // События
        public System.Action<GameObject, BuildingData> OnConstructionComplete;

        public void Initialize(BuildingData data, Inventory inventory)
        {
            buildingData = data;
            playerInventory = inventory;

            // Получаем все рендереры
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

            // Устанавливаем полупрозрачность
            SetTransparency(0.5f);

            // Создаем UI
            CreateConstructionUI();

            // Показываем информацию
            ShowResourceRequirements();
        }

        private void SetTransparency(float alpha)
        {
            foreach (var renderer in spriteRenderers)
            {
                Color color = renderer.color;
                color.a = alpha;
                renderer.color = color;
            }
        }

        private void CreateConstructionUI()
        {
            // Создаем World Canvas
            GameObject canvasGO = new GameObject("Construction Canvas");
            canvasGO.transform.SetParent(transform);

            worldCanvas = canvasGO.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.sortingLayerName = "UI";
            worldCanvas.sortingOrder = 101;

            RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(200, 100);

            // Позиционируем над зданием
            float yOffset = (buildingData.size.y * 0.5f) + 1.5f;
            canvasGO.transform.localPosition = new Vector3(0, yOffset, 0);

            // Панель информации
            GameObject panel = new GameObject("Info Panel");
            panel.transform.SetParent(canvasGO.transform);

            RectTransform panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchoredPosition = Vector2.zero;
            panelRT.sizeDelta = new Vector2(180, 60);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

            // Текст информации
            GameObject textGO = new GameObject("Info Text");
            textGO.transform.SetParent(panel.transform);

            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchoredPosition = Vector2.zero;
            textRT.sizeDelta = new Vector2(160, 40);

            infoText = textGO.AddComponent<TextMeshProUGUI>();
            infoText.text = "Нажмите F для внесения ресурсов";
            infoText.fontSize = 14;
            infoText.alignment = TextAlignmentOptions.Center;
            infoText.color = Color.white;
        }

        private void ShowResourceRequirements()
        {
            string requirements = "Требуется:\n";
            foreach (var req in buildingData.resourceRequirements)
            {
                requirements += $"{req.resource.itemName}: {req.amount}\n";
            }

            infoText.text = requirements + "\nНажмите F для строительства";
        }

        private void Update()
        {
            // Поворачиваем UI к камере
            if (worldCanvas != null && Camera.main != null)
            {
                worldCanvas.transform.rotation = Camera.main.transform.rotation;
            }

            // Проверка взаимодействия
            if (!resourcesDelivered && Input.GetKeyDown(KeyCode.F))
            {
                TryDeliverResources();
            }

            // Процесс строительства
            if (isConstructing)
            {
                constructionProgress += Time.deltaTime / buildingData.buildTime;
                UpdateProgressBar();

                if (constructionProgress >= 1f)
                {
                    CompleteConstruction();
                }
            }
        }

        private void TryDeliverResources()
        {
            // Проверяем расстояние до игрока
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;

            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance > 3f) // Максимальное расстояние для взаимодействия
            {
                ShowMessage("Подойдите ближе!");
                return;
            }

            // Проверяем ресурсы
            if (!buildingData.CanAfford(playerInventory))
            {
                ShowMessage("Недостаточно ресурсов!");
                return;
            }

            // Забираем ресурсы
            foreach (var requirement in buildingData.resourceRequirements)
            {
                playerInventory.RemoveItem(requirement.resource, requirement.amount);
            }

            // Начинаем строительство
            StartConstruction();
        }

        private void StartConstruction()
        {
            resourcesDelivered = true;
            isConstructing = true;

            // Создаем прогресс-бар
            CreateProgressBar();

            // Обновляем текст
            infoText.text = "Строительство...";

            // Эффекты
            CreateConstructionEffects();

            // Анимация
            transform.DOPunchScale(Vector3.one * 0.1f, 0.5f);
        }

        private void CreateProgressBar()
        {
            GameObject barGO = new GameObject("Progress Bar");
            barGO.transform.SetParent(worldCanvas.transform);

            RectTransform barRT = barGO.AddComponent<RectTransform>();
            barRT.anchoredPosition = new Vector2(0, -40);
            barRT.sizeDelta = new Vector2(150, 20);

            // Фон прогресс-бара
            Image bgImage = barGO.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Заполнение прогресс-бара
            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(barGO.transform);

            RectTransform fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0, 0);
            fillRT.anchorMax = new Vector2(1, 1);
            fillRT.sizeDelta = Vector2.zero;
            fillRT.anchoredPosition = Vector2.zero;

            Image fillImage = fillGO.AddComponent<Image>();
            fillImage.color = Color.green;

            progressBar = barGO.AddComponent<Slider>();
            progressBar.fillRect = fillRT;
            progressBar.targetGraphic = fillImage;
            progressBar.value = 0;
        }

        private void UpdateProgressBar()
        {
            if (progressBar != null)
            {
                progressBar.value = constructionProgress;
            }

            // Постепенно увеличиваем прозрачность
            float alpha = Mathf.Lerp(0.5f, 1f, constructionProgress);
            SetTransparency(alpha);
        }

        private void CreateConstructionEffects()
        {
            // Создаем систему частиц для эффекта строительства
            GameObject particlesGO = new GameObject("Construction Particles");
            particlesGO.transform.SetParent(transform);
            particlesGO.transform.localPosition = Vector3.zero;

            constructionParticles = particlesGO.AddComponent<ParticleSystem>();
            var main = constructionParticles.main;
            main.duration = buildingData.buildTime;
            main.loop = true;
            main.startLifetime = 1f;
            main.startSpeed = 2f;
            main.maxParticles = 50;

            var emission = constructionParticles.emission;
            emission.rateOverTime = 10;

            var shape = constructionParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(buildingData.size.x, buildingData.size.y, 1);

            var renderer = constructionParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        private void CompleteConstruction()
        {
            isConstructing = false;

            // Полная непрозрачность
            SetTransparency(1f);

            // Удаляем UI
            if (worldCanvas != null)
                Destroy(worldCanvas.gameObject);

            // Останавливаем частицы
            if (constructionParticles != null)
            {
                constructionParticles.Stop();
                Destroy(constructionParticles.gameObject, 2f);
            }

            // Эффект завершения
            transform.DOPunchScale(Vector3.one * 0.2f, 0.5f)
                .OnComplete(() => {
                    // Вызываем событие завершения
                    OnConstructionComplete?.Invoke(gameObject, buildingData);

                    // Удаляем этот компонент
                    Destroy(this);
                });
        }

        private void ShowMessage(string message)
        {
            infoText.text = message;
            infoText.color = Color.red;

            // Возвращаем исходный текст через 2 секунды
            DOVirtual.DelayedCall(2f, () => {
                ShowResourceRequirements();
                infoText.color = Color.white;
            });
        }
    }
}