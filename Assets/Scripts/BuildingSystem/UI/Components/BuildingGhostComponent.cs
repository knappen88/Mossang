using BuildingSystem.Config;
using UnityEngine;

public class BuildingGhostComponent : MonoBehaviour
{
    private BuildingData buildingData;
    private BuildingSystemConfig config;
    private SpriteRenderer[] spriteRenderers;
    private Material ghostMaterial;
    private bool isValid = false;

    [Header("Resource Display")]
    [SerializeField] private GameObject resourceDisplayPrefab;
    private GameObject resourceDisplay;
    private Canvas worldCanvas;

    public void Initialize(BuildingData data, BuildingSystemConfig systemConfig)
    {
        buildingData = data;
        config = systemConfig;

        SetupGhostVisuals();
        CreateResourceDisplay();
    }

    private void SetupGhostVisuals()
    {
        // Get all sprite renderers
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        // Create or use ghost material
        if (config.GhostMaterial != null)
        {
            ghostMaterial = new Material(config.GhostMaterial);
        }
        else
        {
            // Create default ghost material
            ghostMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        // Apply to all renderers
        foreach (var renderer in spriteRenderers)
        {
            renderer.material = ghostMaterial;
        }

        // Disable all colliders
        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    private void CreateResourceDisplay()
    {
        if (resourceDisplayPrefab == null) return;

        // Create world canvas
        var canvasGO = new GameObject("ResourceCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = Vector3.up * (buildingData.Size.y * 0.5f + 0.5f);

        worldCanvas = canvasGO.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingLayerName = "UI";

        var rect = canvasGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(3, 1);
        rect.localScale = Vector3.one * 0.01f;

        // Create display
        resourceDisplay = Instantiate(resourceDisplayPrefab, worldCanvas.transform);
        UpdateResourceDisplay();
    }

    public void SetValid(bool valid)
    {
        if (isValid == valid) return;

        isValid = valid;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        var targetColor = isValid ? config.ValidPlacementTint : config.InvalidPlacementTint;
        var targetAlpha = isValid ? config.GhostValidAlpha : config.GhostInvalidAlpha;

        foreach (var renderer in spriteRenderers)
        {
            renderer.DOKill();
            renderer.DOColor(targetColor, 0.1f);
            renderer.DOFade(targetAlpha, 0.1f);
        }
    }

    private void UpdateResourceDisplay()
    {
        if (resourceDisplay == null) return;

        // Update resource requirements display
        // This would show required resources and whether player has them
    }

    private void Update()
    {
        // Face canvas to camera
        if (worldCanvas != null && Camera.main != null)
        {
            worldCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    private void OnDestroy()
    {
        if (ghostMaterial != null)
        {
            Destroy(ghostMaterial);
        }
    }
}