using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ConstructionSiteComponent : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject constructionUIPrefab;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Visual Settings")]
    [SerializeField] private float constructionAlpha = 0.5f;
    [SerializeField] private ParticleSystem constructionParticles;

    private BuildingData buildingData;
    private GameObject uiInstance;
    private SpriteRenderer[] spriteRenderers;
    private System.Func<GameObject, float> getProgress;

    public void Initialize(BuildingData data, System.Func<GameObject, float> progressGetter)
    {
        buildingData = data;
        getProgress = progressGetter;

        // Setup visuals
        SetupConstructionVisuals();

        // Create UI
        CreateConstructionUI();

        // Start effects
        StartConstructionEffects();
    }

    private void SetupConstructionVisuals()
    {
        // Get all sprite renderers
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        // Set transparency
        foreach (var renderer in spriteRenderers)
        {
            var color = renderer.color;
            color.a = constructionAlpha;
            renderer.color = color;
        }

        // Disable colliders during construction
        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    private void CreateConstructionUI()
    {
        // Create world space canvas if needed
        if (worldCanvas == null)
        {
            var canvasGO = new GameObject("ConstructionCanvas");
            canvasGO.transform.SetParent(transform);
            canvasGO.transform.localPosition = Vector3.up * (buildingData.Size.y * 0.5f + 1f);

            worldCanvas = canvasGO.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.sortingLayerName = "UI";

            var rect = canvasGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(2, 1);
            rect.localScale = Vector3.one * 0.01f;
        }

        // Create UI from prefab if available
        if (constructionUIPrefab != null)
        {
            uiInstance = Instantiate(constructionUIPrefab, worldCanvas.transform);
            progressBar = uiInstance.GetComponentInChildren<Slider>();
            progressText = uiInstance.GetComponentInChildren<TextMeshProUGUI>();
        }
        else
        {
            // Create simple progress bar
            CreateSimpleProgressBar();
        }
    }

    private void CreateSimpleProgressBar()
    {
        // Create background
        var bgGO = new GameObject("ProgressBG");
        bgGO.transform.SetParent(worldCanvas.transform);
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.5f);

        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(150, 20);
        bgRect.anchoredPosition = Vector2.zero;

        // Create fill
        var fillGO = new GameObject("ProgressFill");
        fillGO.transform.SetParent(bgGO.transform);
        var fillImage = fillGO.AddComponent<Image>();
        fillImage.color = Color.green;

        var fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.sizeDelta = new Vector2(0, 0);
        fillRect.anchoredPosition = Vector2.zero;

        // Setup slider component
        var sliderGO = new GameObject("Slider");
        sliderGO.transform.SetParent(worldCanvas.transform);
        progressBar = sliderGO.AddComponent<Slider>();
        progressBar.fillRect = fillRect;
        progressBar.targetGraphic = fillImage;
        progressBar.value = 0;
    }

    private void StartConstructionEffects()
    {
        if (constructionParticles == null)
        {
            // Create simple particle effect
            var particleGO = new GameObject("ConstructionParticles");
            particleGO.transform.SetParent(transform);
            particleGO.transform.localPosition = Vector3.zero;

            constructionParticles = particleGO.AddComponent<ParticleSystem>();
            var main = constructionParticles.main;
            main.loop = true;
            main.startLifetime = 2f;
            main.startSpeed = 2f;
            main.maxParticles = 50;

            var emission = constructionParticles.emission;
            emission.rateOverTime = 10;

            var shape = constructionParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(buildingData.Size.x, buildingData.Size.y, 0.1f);
        }
    }

    private void Update()
    {
        if (getProgress != null)
        {
            UpdateProgress(getProgress(gameObject));
        }

        // Face canvas to camera
        if (worldCanvas != null && Camera.main != null)
        {
            worldCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null)
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";

        // Update building transparency based on progress
        foreach (var renderer in spriteRenderers)
        {
            var color = renderer.color;
            color.a = Mathf.Lerp(constructionAlpha, 1f, progress);
            renderer.color = color;
        }
    }

    public void OnConstructionComplete()
    {
        // Cleanup UI
        if (uiInstance != null)
            Destroy(uiInstance);

        if (worldCanvas != null)
            Destroy(worldCanvas.gameObject);

        // Stop particles
        if (constructionParticles != null)
        {
            constructionParticles.Stop();
            Destroy(constructionParticles.gameObject, 2f);
        }

        // Restore full opacity
        foreach (var renderer in spriteRenderers)
        {
            var color = renderer.color;
            color.a = 1f;
            renderer.DOColor(color, 0.5f);
        }

        // Re-enable colliders
        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }

        // Add completion effect
        transform.DOPunchScale(Vector3.one * 0.1f, 0.5f);
    }
}