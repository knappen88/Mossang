using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
namespace BuildingSystem.Systems
{
    public class BuildingFactory : IBuildingFactory
    {
        private readonly Transform buildingContainer;
        private readonly BuildingSystemConfig config;
        private readonly Dictionary<string, ObjectPool<GameObject>> buildingPools;
        private readonly ObjectPool<GameObject> ghostPool;

        public BuildingFactory(Transform buildingContainer, BuildingSystemConfig config)
        {
            this.buildingContainer = buildingContainer;
            this.config = config;
            this.buildingPools = new Dictionary<string, ObjectPool<GameObject>>();

            // Create ghost pool
            ghostPool = new ObjectPool<GameObject>(
                createFunc: CreateGhostObject,
                actionOnGet: OnGetGhost,
                actionOnRelease: OnReleaseGhost,
                actionOnDestroy: OnDestroyObject,
                defaultCapacity: 5,
                maxSize: 10
            );
        }

        public GameObject CreateBuilding(BuildingData data, Vector3 position, Quaternion rotation)
        {
            // Get from pool if available
            if (!buildingPools.ContainsKey(data.Id))
            {
                buildingPools[data.Id] = CreateBuildingPool(data);
            }

            var building = buildingPools[data.Id].Get();
            building.transform.position = position;
            building.transform.rotation = rotation;
            building.name = $"{data.BuildingName}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

            // Initialize building controller
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
            var model = Object.Instantiate(data.GhostPrefab, ghost.transform);
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

        private ObjectPool<GameObject> CreateBuildingPool(BuildingData data)
        {
            return new ObjectPool<GameObject>(
                createFunc: () => CreateBuildingObject(data),
                actionOnGet: obj => OnGetBuilding(obj),
                actionOnRelease: obj => OnReleaseBuilding(obj),
                actionOnDestroy: obj => OnDestroyObject(obj),
                defaultCapacity: 5,
                maxSize: 20
            );
        }

        private GameObject CreateBuildingObject(BuildingData data)
        {
            var building = Object.Instantiate(data.Prefab, buildingContainer);
            building.SetActive(false);
            return building;
        }

        private GameObject CreateGhostObject()
        {
            var ghost = new GameObject("BuildingGhost");
            ghost.transform.SetParent(buildingContainer);
            ghost.SetActive(false);
            return ghost;
        }

        private void OnGetBuilding(GameObject building)
        {
            building.SetActive(true);
        }

        private void OnReleaseBuilding(GameObject building)
        {
            building.SetActive(false);
            building.transform.position = Vector3.zero;
            building.transform.rotation = Quaternion.identity;
        }

        private void OnGetGhost(GameObject ghost)
        {
            ghost.SetActive(true);
        }

        private void OnReleaseGhost(GameObject ghost)
        {
            ghost.SetActive(false);
            ghost.transform.position = Vector3.zero;
            ghost.transform.rotation = Quaternion.identity;
        }

        private void OnDestroyObject(GameObject obj)
        {
            if (obj != null)
                Object.Destroy(obj);
        }
    }
}