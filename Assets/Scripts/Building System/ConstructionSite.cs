using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

namespace Building
{
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

        public System.Action<GameObject, BuildingData> OnConstructionComplete;

        public void Initialize(BuildingData data, Inventory inventory)
        {
            buildingData = data;
            playerInventory = inventory;

            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

            SetTransparency(0.5f);

            CreateConstructionUI();

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
            GameObject canvasGO = new GameObject("Construction Canvas");
            canvasGO.transform.SetParent(transform);

            worldCanvas = canvasGO.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.sortingLayerName = "UI";
            worldCanvas.sortingOrder = 101;

            RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(200, 100);

            float yOffset = (buildingData.size.y * 0.5f) + 1.5f;
            canvasGO.transform.localPosition = new Vector3(0, yOffset, 0);

            GameObject panel = new GameObject("Info Panel");
            panel.transform.SetParent(canvasGO.transform);

            RectTransform panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchoredPosition = Vector2.zero;
            panelRT.sizeDelta = new Vector2(180, 60);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

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
            if (worldCanvas != null && Camera.main != null)
            {
                worldCanvas.transform.rotation = Camera.main.transform.rotation;
            }

            if (!resourcesDelivered && Input.GetKeyDown(KeyCode.F))
            {
                TryDeliverResources();
            }

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
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;

            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance > 3f)
            {
                ShowMessage("Подойдите ближе!");
                return;
            }

            if (!buildingData.CanAfford(playerInventory))
            {
                ShowMessage("Недостаточно ресурсов!");
                return;
            }

            foreach (var requirement in buildingData.resourceRequirements)
            {
                playerInventory.RemoveItem(requirement.resource, requirement.amount);
            }

            StartConstruction();
        }

        private void StartConstruction()
        {
            resourcesDelivered = true;
            isConstructing = true;

            CreateProgressBar();

            infoText.text = "Строительство...";

            CreateConstructionEffects();

            transform.DOPunchScale(Vector3.one * 0.1f, 0.5f);
        }

        private void CreateProgressBar()
        {
            GameObject barGO = new GameObject("Progress Bar");
            barGO.transform.SetParent(worldCanvas.transform);

            RectTransform barRT = barGO.AddComponent<RectTransform>();
            barRT.anchoredPosition = new Vector2(0, -40);
            barRT.sizeDelta = new Vector2(150, 20);

            Image bgImage = barGO.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

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

            float alpha = Mathf.Lerp(0.5f, 1f, constructionProgress);
            SetTransparency(alpha);
        }

        private void CreateConstructionEffects()
        {
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

            SetTransparency(1f);

            if (worldCanvas != null)
                Destroy(worldCanvas.gameObject);

            if (constructionParticles != null)
            {
                constructionParticles.Stop();
                Destroy(constructionParticles.gameObject, 2f);
            }

            transform.DOPunchScale(Vector3.one * 0.2f, 0.5f)
                .OnComplete(() => {
                    OnConstructionComplete?.Invoke(gameObject, buildingData);

                    Destroy(this);
                });
        }

        private void ShowMessage(string message)
        {
            infoText.text = message;
            infoText.color = Color.red;

            DOVirtual.DelayedCall(2f, () => {
                ShowResourceRequirements();
                infoText.color = Color.white;
            });
        }
    }
}