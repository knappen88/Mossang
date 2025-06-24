using UnityEngine;
using System;

namespace BuildingSystem.Components
{
    public class ConstructionSiteComponent : MonoBehaviour
    {
        private BuildingData buildingData;
        private Func<GameObject, float> progressGetter;
        private SpriteRenderer[] spriteRenderers;
        private ParticleSystem constructionParticles;

        public void Initialize(BuildingData data, Func<GameObject, float> getProgress)
        {
            buildingData = data;
            progressGetter = getProgress;
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

            CreateConstructionVisuals();
        }

        private void Update()
        {
            if (progressGetter != null)
            {
                UpdateVisuals(progressGetter(gameObject));
            }
        }

        public void UpdateVisuals(float progress)
        {
            // Update transparency based on progress
            foreach (var renderer in spriteRenderers)
            {
                var color = renderer.color;
                color.a = 0.3f + (0.7f * progress);
                renderer.color = color;
            }

            // Update particle emission
            if (constructionParticles != null)
            {
                var emission = constructionParticles.emission;
                emission.rateOverTime = Mathf.Lerp(10f, 2f, progress);
            }
        }

        private void CreateConstructionVisuals()
        {
            // Create construction particles
            var particlesObj = new GameObject("ConstructionParticles");
            particlesObj.transform.SetParent(transform);
            particlesObj.transform.localPosition = Vector3.zero;

            constructionParticles = particlesObj.AddComponent<ParticleSystem>();
            var main = constructionParticles.main;
            main.startLifetime = 2f;
            main.startSpeed = 2f;
            main.maxParticles = 50;

            var shape = constructionParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(buildingData.Size.x, buildingData.Size.y, 1f);

            var emission = constructionParticles.emission;
            emission.rateOverTime = 10f;

            // Make sprites semi-transparent
            foreach (var renderer in spriteRenderers)
            {
                var color = renderer.color;
                color.a = 0.3f;
                renderer.color = color;
            }
        }

        private void OnDestroy()
        {
            if (constructionParticles != null)
            {
                Destroy(constructionParticles.gameObject);
            }
        }
    }
}