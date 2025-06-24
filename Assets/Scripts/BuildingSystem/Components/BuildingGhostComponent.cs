using UnityEngine;
using BuildingSystem.Config;

namespace BuildingSystem.Components
{
    public class BuildingGhostComponent : MonoBehaviour
    {
        private BuildingData buildingData;
        private BuildingSystemConfig config;
        private SpriteRenderer[] spriteRenderers;
        private Material ghostMaterial;
        private Collider2D[] colliders;

        public void Initialize(BuildingData data, BuildingSystemConfig systemConfig)
        {
            buildingData = data;
            config = systemConfig;
            
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            colliders = GetComponentsInChildren<Collider2D>();
            
            SetupGhostMaterial();
            DisableColliders();
        }

        private void SetupGhostMaterial()
        {
            // Create semi-transparent material for ghost
            ghostMaterial = new Material(Shader.Find("Sprites/Default"));
            
            foreach (var renderer in spriteRenderers)
            {
                renderer.material = ghostMaterial;
                renderer.sortingOrder += 100; // Place above other sprites
            }
            
            SetValid(true);
        }

        private void DisableColliders()
        {
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }
        }

        public void SetValid(bool isValid)
        {
            var color = isValid ? config.ValidPlacementColor : config.InvalidPlacementColor;
            color.a = 0.7f;

            foreach (var renderer in spriteRenderers)
            {
                renderer.color = color;
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
}