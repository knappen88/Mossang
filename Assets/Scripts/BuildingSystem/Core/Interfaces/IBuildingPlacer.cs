
using UnityEngine;
using System.Collections.Generic;

namespace BuildingSystem.Core.Interfaces
{
    public interface IBuildingPlacer
    {
        void StartPlacement(BuildingData buildingData);
        void UpdatePlacement(Vector3 pointerWorldPosition);
        void ConfirmPlacement();
        void CancelPlacement();
        void RotateGhost(int rotationDelta);
        bool IsPlacing { get; }
        bool CanPlace { get; }
    }
}