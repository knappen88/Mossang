using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Building
{
    public class BuildingController : MonoBehaviour
    {
        private BuildingData buildingData;
        private bool isSelected = false;
        private SpriteRenderer[] spriteRenderers;
        private Color[] originalColors;

        [Header("Interaction")]
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private Color hoverColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        [SerializeField] private Color selectedColor = new Color(1.2f, 1.2f, 1f, 1f);

        [Header("Visual Effects")]
        [SerializeField] private GameObject selectionIndicator;
        [SerializeField] private ParticleSystem idleParticles;

        public System.Action<BuildingController> OnBuildingClicked;
        public System.Action<BuildingController> OnBuildingHovered;

        public BuildingData Data => buildingData;

        public void Initialize(BuildingData data)
        {
            buildingData = data;

            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            originalColors = new Color[spriteRenderers.Length];

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                originalColors[i] = spriteRenderers[i].color;
            }

            CreateBuildingEffects();

            if (GetComponent<Collider2D>() == null)
            {
                BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(buildingData.size.x, buildingData.size.y);
            }
        }

        private void CreateBuildingEffects()
        {
            switch (buildingData.category)
            {
                case BuildingCategory.Production:
                    CreateProductionEffects();
                    break;

                case BuildingCategory.Crafting:
                    CreateCraftingEffects();
                    break;

                case BuildingCategory.Storage:
                    break;
            }
        }

        private void CreateProductionEffects()
        {
            if (idleParticles == null)
            {
                GameObject smokeGO = new GameObject("Smoke");
                smokeGO.transform.SetParent(transform);
                smokeGO.transform.localPosition = new Vector3(0, buildingData.size.y * 0.5f, -0.1f);

                idleParticles = smokeGO.AddComponent<ParticleSystem>();
                var main = idleParticles.main;
                main.loop = true;
                main.startLifetime = 2f;
                main.startSpeed = 1f;
                main.maxParticles = 20;

                var emission = idleParticles.emission;
                emission.rateOverTime = 2;

                var shape = idleParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.1f;

                var velocityOverLifetime = idleParticles.velocityOverLifetime;
                velocityOverLifetime.enabled = true;
                velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(1f);

                var colorOverLifetime = idleParticles.colorOverLifetime;
                colorOverLifetime.enabled = true;
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(Color.gray, 0.0f),
                        new GradientColorKey(Color.gray, 1.0f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0.5f, 0.0f),
                        new GradientAlphaKey(0.0f, 1.0f)
                    }
                );
                colorOverLifetime.color = gradient;
            }
        }

        private void CreateCraftingEffects()
        {
        }

        private void OnMouseEnter()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            SetHighlight(true);
            OnBuildingHovered?.Invoke(this);
        }

        private void OnMouseExit()
        {
            if (!isSelected)
                SetHighlight(false);
        }

        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance <= interactionDistance)
                {
                    OnBuildingClicked?.Invoke(this);

                    transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
                }
                else
                {
                    ShowTooFarEffect();
                }
            }
        }

        private void SetHighlight(bool highlight)
        {
            Color targetColor = highlight ? hoverColor : Color.white;

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].DOColor(originalColors[i] * targetColor, 0.2f);
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;

            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(selected);
            }

            Color targetColor = selected ? selectedColor : Color.white;
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].DOColor(originalColors[i] * targetColor, 0.2f);
            }
        }

        private void ShowTooFarEffect()
        {
            foreach (var renderer in spriteRenderers)
            {
                renderer.DOColor(Color.red, 0.1f)
                    .OnComplete(() => renderer.DOColor(originalColors[0], 0.2f));
            }

            transform.DOShakePosition(0.3f, 0.1f, 10);
        }

        public void OpenBuildingMenu()
        {
            Debug.Log($"[BuildingController] Opening menu for: {buildingData.buildingName}");

            switch (buildingData.category)
            {
                case BuildingCategory.Storage:
                    break;

                case BuildingCategory.Production:
                    break;

                case BuildingCategory.Crafting:
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);

            Gizmos.color = Color.green;
            if (buildingData != null)
            {
                Vector3 size = new Vector3(buildingData.size.x, buildingData.size.y, 0);
                Gizmos.DrawWireCube(transform.position, size);
            }
        }
    }
}