using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using BuildingSystem.Core.Interfaces;
using BuildingSystem.Config;
using BuildingSystem.Controllers;
using BuildingSystem.Components;

namespace BuildingSystem.Systems
{
    public class BuildingFactory : IBuildingFactory
    {
        private readonly Transform buildingContainer;
        private readonly BuildingSystemConfig config;
        private readonly Dictionary<string, ObjectPool<GameObject>> buildingPools;
        private readonly ObjectPool<GameObject> ghostPool;

        public BuildingFactory(Transform container, BuildingSystemConfig systemConfig)
        {
            buildingContainer = container;
            config = systemConfig;
            buildingPools = new Dictionary<string, ObjectPool<GameObject>>();

            // Create ghost pool
            ghostPool = new ObjectPool<GameObject>(
                createFunc: CreateGhostObject,
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Object.Destroy(obj),
                collectionCheck: false,
                defaultCapacity: 5,
                maxSize: 10
            );
        }

        public GameObject CreateBuilding(BuildingData data, Vector3 position, Quaternion rotation)
        {
            // Get or create pool for this building type
            if (!buildingPools.ContainsKey(data.Id))
            {
                CreateBuildingPool(data);
            }

            var building = buildingPools[data.Id].Get();
            building.transform.position = position;
            building.transform.rotation = rotation;

            // Setup building controller
            var controller = building.GetComponent<BuildingController>();
            if (controller == null)
            {
                controller = building.AddComponent<BuildingController>();
            }
            controller.Initialize(data);

            return building;
        }

        public GameObject CreateGhost(BuildingData data)
        {
            var ghost = ghostPool.Get();

            // Replace with building model
            var model = Object.Instantiate(data.GhostPrefab ?? data.Prefab, ghost.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            // Add ghost component
            var ghostComponent = ghost.GetComponent<BuildingGhostComponent>();
            if (ghostComponent == null)
            {
                ghostComponent = ghost.AddComponent<BuildingGhostComponent>();
            }
            ghostComponent.Initialize(data, config);

            return ghost;
        }

        public void RecycleBuilding(GameObject building)
        {
            var controller = building.GetComponent<BuildingController>();
            if (controller != null && controller.Data != null)
            {
                var poolKey = controller.Data.Id;
                if (buildingPools.ContainsKey(poolKey))
                {
                    buildingPools[poolKey].Release(building);
                    return;
                }
            }

            // If no pool found, destroy
            Object.Destroy(building);
        }

        public void RecycleGhost(GameObject ghost)
        {
            // Clear any child models
            foreach (Transform child in ghost.transform)
            {
                Object.Destroy(child.gameObject);
            }

            ghostPool.Release(ghost);
        }

        private void CreateBuildingPool(BuildingData data)
        {
            var pool = new ObjectPool<GameObject>(
                createFunc: () => CreateBuildingObject(data),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Object.Destroy(obj),
                collectionCheck: false,
                defaultCapacity: config.DefaultPoolSize,
                maxSize: config.MaxPoolSize
            );

            buildingPools[data.Id] = pool;
        }

        private GameObject CreateBuildingObject(BuildingData data)
        {
            var building = Object.Instantiate(data.Prefab, buildingContainer);
            building.name = $"{data.BuildingName}_{System.Guid.NewGuid()}";
            return building;
        }

        private GameObject CreateGhostObject()
        {
            var ghost = new GameObject("BuildingGhost");
            ghost.transform.SetParent(buildingContainer);
            return ghost;
        }
    }
}