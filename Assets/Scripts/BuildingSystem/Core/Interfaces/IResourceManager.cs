using System.Collections.Generic;
using BuildingSystem.Core;

namespace BuildingSystem.Core.Interfaces
{
    public interface IResourceManager
    {
        bool HasResources(IEnumerable<ResourceRequirement> requirements);
        bool ConsumeResources(IEnumerable<ResourceRequirement> requirements);
        void RefundResources(IEnumerable<ResourceRequirement> requirements);
        int GetResourceAmount(string resourceId);
    }
}