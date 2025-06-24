using UnityEngine;

namespace BuildingSystem.Components
{
    public class StorageBuildingComponent : MonoBehaviour
    {
        private BuildingData buildingData;

        public void Initialize(BuildingData data)
        {
            buildingData = data;
            ExpandStorage();
        }

        private void ExpandStorage()
        {
            if (buildingData.ResourceStorage == null) return;

            foreach (var storage in buildingData.ResourceStorage)
            {
                Debug.Log($"Building {gameObject.name} added {storage.capacity} storage capacity for {storage.resourceId}");
                
                // Expand player storage capacity
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    // You'll need to implement this based on your inventory system
                    // Example: player.GetComponent<Inventory>().ExpandCapacity(storage.resourceId, storage.capacity);
                }
            }
        }

        private void OnDestroy()
        {
            // Remove storage capacity when building is destroyed
            if (buildingData.ResourceStorage == null) return;

            foreach (var storage in buildingData.ResourceStorage)
            {
                Debug.Log($"Building {gameObject.name} removed {storage.capacity} storage capacity for {storage.resourceId}");
                
                // Reduce player storage capacity
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    // You'll need to implement this based on your inventory system
                    // Example: player.GetComponent<Inventory>().ReduceCapacity(storage.resourceId, storage.capacity);
                }
            }
        }
    }
}