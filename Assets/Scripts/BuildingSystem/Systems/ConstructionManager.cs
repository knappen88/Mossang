using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BuildingSystem.Core.Interfaces;
using BuildingSystem.Core.Events;
using BuildingSystem.Config;
using BuildingSystem.Components;
using BuildingSystem.Controllers;
using BuildingSystem.Core;

namespace BuildingSystem.Systems
{
    public class ConstructionManager : IConstructionManager
    {
        private readonly BuildingEventChannel eventChannel;
        private readonly BuildingSystemConfig config;
        private readonly IResourceManager resourceManager;
        private readonly Dictionary<GameObject, ConstructionData> activeConstructions;

        private class ConstructionData
        {
            public BuildingData BuildingData { get; set; }
            public float StartTime { get; set; }
            public float Duration { get; set; }
            public ConstructionSiteComponent Component { get; set; }
        }

        public ConstructionManager(BuildingEventChannel eventChannel, BuildingSystemConfig config, IResourceManager resourceManager = null)
        {
            this.eventChannel = eventChannel;
            this.config = config;
            this.resourceManager = resourceManager;
            this.activeConstructions = new Dictionary<GameObject, ConstructionData>();
        }

        public void StartConstruction(GameObject building, BuildingData data)
        {
            if (activeConstructions.ContainsKey(building))
            {
                Debug.LogWarning($"Building {building.name} is already under construction!");
                return;
            }

            // Check for instant construction
            var constructionTime = config.InstantConstruction ? 0f : data.ConstructionTime * config.ConstructionSpeedMultiplier;

            // Add construction site component
            var constructionSite = building.GetComponent<ConstructionSiteComponent>();
            if (constructionSite == null)
            {
                constructionSite = building.AddComponent<ConstructionSiteComponent>();
            }

            constructionSite.Initialize(data, GetConstructionProgress);

            // Register construction
            var constructionData = new ConstructionData
            {
                BuildingData = data,
                StartTime = Time.time,
                Duration = constructionTime,
                Component = constructionSite
            };

            activeConstructions[building] = constructionData;

            // Publish event
            eventChannel.Publish(new BuildingConstructionStartedEvent(building, constructionTime));

            // If instant construction, complete immediately
            if (constructionTime <= 0f)
            {
                CompleteConstruction(building);
            }
        }

        public void UpdateConstruction(GameObject building, float deltaTime)
        {
            if (!activeConstructions.TryGetValue(building, out var data))
                return;

            var progress = GetConstructionProgress(building);
            if (data.Component != null)
            {
                data.Component.UpdateVisuals(progress);
            }

            if (progress >= 1f)
            {
                CompleteConstruction(building);
            }
        }

        public bool IsUnderConstruction(GameObject building)
        {
            return activeConstructions.ContainsKey(building);
        }

        public float GetConstructionProgress(GameObject building)
        {
            if (!activeConstructions.TryGetValue(building, out var data))
                return 0f;

            if (data.Duration <= 0f)
                return 1f;

            var elapsed = Time.time - data.StartTime;
            return Mathf.Clamp01(elapsed / data.Duration);
        }

        public void CompleteConstruction(GameObject building)
        {
            if (!activeConstructions.TryGetValue(building, out var data))
                return;

            // Remove construction site component
            if (data.Component != null)
                Object.Destroy(data.Component);

            // Notify building controller
            var controller = building.GetComponent<BuildingController>();
            controller?.OnConstructionComplete();

            // Remove from active constructions
            activeConstructions.Remove(building);

            // Publish event
            eventChannel.Publish(new BuildingConstructionCompletedEvent(building, data.BuildingData));
        }

        public void CancelConstruction(GameObject building)
        {
            if (!activeConstructions.TryGetValue(building, out var data))
                return;

            // Calculate refund based on progress
            var progress = GetConstructionProgress(building);
            var refundPercent = 1f - (progress * 0.5f); // 100% refund at start, 50% at completion

            // Remove construction site component
            if (data.Component != null)
                Object.Destroy(data.Component);

            // Remove from active constructions
            activeConstructions.Remove(building);

            // Refund resources if resource manager is available
            if (resourceManager != null && data.BuildingData.ResourceRequirements != null)
            {
                var refundRequirements = data.BuildingData.ResourceRequirements
                    .Select(r => new ResourceRequirement(r.resourceId, Mathf.FloorToInt(r.amount * refundPercent)))
                    .Where(r => r.amount > 0)
                    .ToArray();

                resourceManager.RefundResources(refundRequirements);
            }

            // Destroy the building
            Object.Destroy(building);
        }
    }
}