using System.Collections.Generic;
using UnityEngine;
namespace BuildingSystem.Controllers
{
    public class InventoryAdapter : IResourceManager
{
    private readonly Inventory legacyInventory;

    public InventoryAdapter(Inventory inventory)
    {
        legacyInventory = inventory;
    }

    public bool HasResources(IEnumerable<ResourceRequirement> requirements)
    {
        if (legacyInventory == null) return false;

        foreach (var req in requirements)
        {
            var count = GetResourceCount(req.resourceId);
            if (count < req.amount) return false;
        }
        return true;
    }

    public bool ConsumeResources(IEnumerable<ResourceRequirement> requirements)
    {
        if (!HasResources(requirements)) return false;

        foreach (var req in requirements)
        {
            RemoveResource(req.resourceId, req.amount);
        }
        return true;
    }

    public void RefundResources(IEnumerable<ResourceRequirement> requirements)
    {
        foreach (var req in requirements)
        {
            AddResource(req.resourceId, req.amount);
        }
    }

    public int GetResourceAmount(string resourceId)
    {
        return GetResourceCount(resourceId);
    }

    private int GetResourceCount(string resourceId)
    {
        int count = 0;
        foreach (var item in legacyInventory.items)
        {
            if (item.itemData.name == resourceId || item.itemData.itemID == resourceId)
            {
                count += item.quantity;
            }
        }
        return count;
    }

    private void RemoveResource(string resourceId, int amount)
    {
        foreach (var item in legacyInventory.items)
        {
            if (item.itemData.name == resourceId || item.itemData.itemID == resourceId)
            {
                legacyInventory.RemoveItem(item.itemData, amount);
                break;
            }
        }
    }

    private void AddResource(string resourceId, int amount)
    {
        // Find item data by id
        var itemData = FindItemData(resourceId);
        if (itemData != null)
        {
            legacyInventory.AddItem(itemData, amount);
        }
    }

    private ItemData FindItemData(string resourceId)
    {
        // This would need to be implemented based on your item system
        // Could use a central item database or Resources.Load
        return null;
    }
}

// Additional States
public class ConstructionState : BuildingSystemStateBase
{
    public ConstructionState(BuildingSystemContext context) : base(context) { }

    public override void Enter()
    {
        // Construction is handled by ConstructionManager
    }

    public override void Exit() { }

    public override void Update()
    {
        // Update all constructions
        foreach (var building in context.BuildingRepository.GetAllBuildings())
        {
            if (context.ConstructionManager.IsUnderConstruction(building))
            {
                context.ConstructionManager.UpdateConstruction(building, Time.deltaTime);
            }
        }
    }

    public override void HandleInput() { }
}

public class DemolitionState : BuildingSystemStateBase
{
    public DemolitionState(BuildingSystemContext context) : base(context) { }

    public override void Enter()
    {
        // Show demolition cursor/effect
    }

    public override void Exit()
    {
        // Hide demolition cursor/effect
    }

    public override void Update() { }

    public override void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleDemolition();
        }
    }

    private void HandleDemolition()
    {
        var ray = context.MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var building = hit.collider.GetComponent<BuildingController>();
            if (building != null && context.BuildingRepository.TryGetBuildingInfo(building.gameObject, out var info))
            {
                if (info.Data.IsDestructible)
                {
                    DemolishBuilding(building.gameObject, info);
                }
            }
        }
    }

    private void DemolishBuilding(GameObject building, BuildingInfo info)
    {
        // Free grid cells
        context.GridManager.FreeCells(info.GridPosition, info.Data.Size);

        // Refund partial resources
        var refundAmount = info.State == BuildingState.Completed ? 0.5f : 0f;
        if (refundAmount > 0)
        {
            var refundRequirements = info.Data.ResourceRequirements
                .Select(r => new ResourceRequirement(r.resourceId, Mathf.FloorToInt(r.amount * refundAmount)))
                .ToArray();
            context.ResourceManager.RefundResources(refundRequirements);
        }

        // Publish event
        context.EventChannel.Publish(new BuildingDestroyedEvent(building, info.GridPosition));

        // Remove from repository
        context.BuildingRepository.UnregisterBuilding(building);

        // Recycle building
        context.BuildingFactory.RecycleBuilding(building);
    }
}
}