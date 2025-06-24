using UnityEngine;
using System.Collections;

namespace BuildingSystem.Components
{
    public class ProductionBuildingComponent : MonoBehaviour
    {
        private BuildingData buildingData;
        private Coroutine productionCoroutine;
        private float efficiency = 1f;

        public void Initialize(BuildingData data)
        {
            buildingData = data;
            StartProduction();
        }

        private void StartProduction()
        {
            if (buildingData.ResourceProduction != null && buildingData.ResourceProduction.Length > 0)
            {
                productionCoroutine = StartCoroutine(ProductionCycle());
            }
        }

        private IEnumerator ProductionCycle()
        {
            while (true)
            {
                yield return new WaitForSeconds(buildingData.ProductionInterval);
                ProduceResources();
            }
        }

        private void ProduceResources()
        {
            foreach (var production in buildingData.ResourceProduction)
            {
                var amount = Mathf.RoundToInt(production.amountPerCycle * efficiency);
                Debug.Log($"Building {gameObject.name} produced {amount} of {production.resourceId}");
                
                // Add resources to player inventory
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    // You'll need to implement this based on your inventory system
                    // Example: player.GetComponent<Inventory>().AddResource(production.resourceId, amount);
                }
            }
        }

        public void SetEfficiency(float newEfficiency)
        {
            efficiency = Mathf.Clamp01(newEfficiency);
        }

        private void OnDestroy()
        {
            if (productionCoroutine != null)
            {
                StopCoroutine(productionCoroutine);
            }
        }
    }
}