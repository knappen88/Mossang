using System.Collections.Generic;
using UnityEngine;
using BuildingSystem.Core.Interfaces;
using BuildingSystem.Core.Events;
using BuildingSystem.Config;

namespace BuildingSystem.Systems
{
    public class ConstructionManager : IConstructionManager
    {
        private readonly BuildingEventChannel eventChannel;
        private readonly BuildingSystemConfig config;
        private readonly Dictionary<GameObject, ConstructionData> activeConstructions;

        private class ConstructionData
        {
            public BuildingData BuildingData { get; set; }
            public float StartTime { get; set; }
            public float Duration { get; set; }
            public ConstructionSiteComponent Component { get; set; }
        }

        public ConstructionManager(BuildingEventChannel eventChannel, BuildingSystemConfig config)
        {
            this.eventChannel = eventChannel;
            this.config = config;
            this.activeConstructions = new Dictionary<GameObject, ConstructionData>();
        }

        public void StartConstruction(GameObject building, BuildingData data)
        {
            if (activeConstructions.ContainsKey(building))
            {
                Debug.LogWarning($"Building {building.name} is already under construction!");
                return;
            }

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
                Duration = data.ConstructionTime,
                Component = constructionSite
            };

            activeConstructions[building] = constructionData;

            // Publish event
            eventChannel.Publish(new BuildingConstructionStartedEvent(building, data.ConstructionTime));
        }

        public void UpdateConstruction(GameObject building, float deltaTime)
        {
            if (!activeConstructions.TryGetValue(building, out var data))
                return;

            var progress = GetConstructionProgress(building);

            // Update visual
            data.Component?.UpdateProgress(progress);

            // Check if complete
            if (progress >= 1f)
            {
                CompleteConstruction(building);
            }
        }

        public void CompleteConstruction(GameObject building)
        {
            if (!activeConstructions.TryGetValue(building, out var data))
                return;

            // Remove construction site component
            if (data.Component != null)
            {
                data.Component.OnConstructionComplete();
                Object.Destroy(data.Component);
            }

            // Remove from active constructions
            activeConstructions.Remove(building);

            // Enable building functionality
            var controller = building.GetComponent<BuildingController>();
            controller?.OnConstructionComplete();

            // Play sound
            if (config.ConstructionCompleteSound != null)
            {
                AudioSource.PlayClipAtPoint(config.ConstructionCompleteSound, building.transform.position, config.AudioVolume);
            }

            // Publish event
            eventChannel.Publish(new BuildingConstructionCompletedEvent(building, data.BuildingData));
        }

        public void CancelConstruction(GameObject building)
        {
            if (!activeConstructions.TryGetValue(building, out var data))
                return;

            // Remove construction site
            if (data.Component != null)
            {
                Object.Destroy(data.Component);
            }

            activeConstructions.Remove(building);
        }

        public float GetConstructionProgress(GameObject building)
        {
            if (!activeConstructions.TryGetValue(building, out var data))
                return 1f;

            var elapsed = Time.time - data.StartTime;
            return Mathf.Clamp01(elapsed / data.Duration);
        }

        public bool IsUnderConstruction(GameObject building)
        {
            return activeConstructions.ContainsKey(building);
        }
    }