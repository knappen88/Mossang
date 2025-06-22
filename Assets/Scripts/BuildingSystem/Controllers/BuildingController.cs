using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using BuildingSystem.Config;

namespace BuildingSystem.Controllers
{
    public class BuildingController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Settings")]
        [SerializeField] private float interactionDistance = 5f;
        [SerializeField] private Color hoverTint = new Color(1.1f, 1.1f, 1.1f);
        [SerializeField] private Color selectedTint = new Color(1.2f, 1.2f, 1f);

        private BuildingData buildingData;
        private bool isSelected;
        private bool isInteractable = true;
        private SpriteRenderer[] spriteRenderers;
        private Color[] originalColors;

        public BuildingData Data => buildingData;
        public bool IsSelected => isSelected;

        public void Initialize(BuildingData data)
        {
            buildingData = data;

            // Cache sprite renderers
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            originalColors = new Color[spriteRenderers.Length];

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                originalColors[i] = spriteRenderers[i].color;
            }

            // Add collider if missing
            if (GetComponent<Collider2D>() == null)
            {
                var collider = gameObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(data.Size.x, data.Size.y);
            }

            // Start as non-interactable during construction
            SetInteractable(false);
        }

        public void OnConstructionComplete()
        {
            SetInteractable(true);

            // Add any building-specific components based on type
            switch (buildingData.Category)
            {
                case BuildingCategory.Production:
                    gameObject.AddComponent<ProductionBuildingComponent>().Initialize(buildingData);
                    break;
                case BuildingCategory.Storage:
                    gameObject.AddComponent<StorageBuildingComponent>().Initialize(buildingData);
                    break;
            }
        }

        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
        }

        public void SetSelected(bool selected)
        {
            if (!isInteractable) return;

            isSelected = selected;
            UpdateVisuals();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable || EventSystem.current.IsPointerOverGameObject()) return;

            UpdateTint(hoverTint);
            transform.DOScale(1.05f, 0.2f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable) return;

            UpdateVisuals();
            transform.DOScale(1f, 0.2f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable || EventSystem.current.IsPointerOverGameObject()) return;

            // Check distance to player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance > interactionDistance)
                {
                    ShowTooFarEffect();
                    return;
                }
            }

            // Fire selection event
            var eventChannel = Resources.Load<BuildingEventChannel>("BuildingEventChannel");
            eventChannel?.Publish(new BuildingSelectedEvent(gameObject));
        }

        private void UpdateVisuals()
        {
            var tint = isSelected ? selectedTint : Color.white;
            UpdateTint(tint);
        }

        private void UpdateTint(Color tint)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].DOColor(originalColors[i] * tint, 0.2f);
                }
            }
        }

        private void ShowTooFarEffect()
        {
            // Flash red
            foreach (var renderer in spriteRenderers)
            {
                renderer.DOColor(Color.red, 0.1f)
                    .OnComplete(() => renderer.DOColor(originalColors[0], 0.2f));
            }

            // Shake
            transform.DOShakePosition(0.3f, 0.1f, 10);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);

            if (buildingData != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, new Vector3(buildingData.Size.x, buildingData.Size.y, 0));
            }
        }
    }
}
}