using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using BuildingSystem.Core.Events;
using BuildingSystem.Core.Interfaces;
using BuildingSystem.Config;

namespace BuildingSystem.UI.MVVM.ViewModels
{
    public class BuildingMenuViewModel : INotifyPropertyChanged
    {
        private readonly BuildingEventChannel eventChannel;
        private readonly IResourceManager resourceManager;
        private readonly List<BuildingData> allBuildings;

        private BuildingCategory selectedCategory;
        private BuildingData selectedBuilding;
        private bool isMenuOpen;

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<BuildingData> OnBuildingStartPlacement;

        // Properties
        public bool IsMenuOpen
        {
            get => isMenuOpen;
            set
            {
                if (isMenuOpen != value)
                {
                    isMenuOpen = value;
                    OnPropertyChanged();
                }
            }
        }

        public BuildingCategory SelectedCategory
        {
            get => selectedCategory;
            set
            {
                if (selectedCategory != value)
                {
                    selectedCategory = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredBuildings));
                }
            }
        }

        public BuildingData SelectedBuilding
        {
            get => selectedBuilding;
            set
            {
                if (selectedBuilding != value)
                {
                    selectedBuilding = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanAffordSelected));
                    OnPropertyChanged(nameof(SelectedBuildingRequirements));
                }
            }
        }

        public IEnumerable<BuildingCategory> Categories => Enum.GetValues(typeof(BuildingCategory)) as BuildingCategory[];

        public IEnumerable<BuildingData> FilteredBuildings
        {
            get
            {
                foreach (var building in allBuildings)
                {
                    if (building.Category == selectedCategory)
                        yield return building;
                }
            }
        }

        public bool CanAffordSelected
        {
            get
            {
                if (selectedBuilding == null) return false;
                return resourceManager.HasResources(selectedBuilding.ResourceRequirements);
            }
        }

        public IEnumerable<ResourceRequirementViewModel> SelectedBuildingRequirements
        {
            get
            {
                if (selectedBuilding == null) yield break;

                foreach (var req in selectedBuilding.ResourceRequirements)
                {
                    yield return new ResourceRequirementViewModel
                    {
                        ResourceId = req.resourceId,
                        Required = req.amount,
                        Available = resourceManager.GetResourceAmount(req.resourceId),
                        HasEnough = resourceManager.GetResourceAmount(req.resourceId) >= req.amount
                    };
                }
            }
        }

        // Constructor
        public BuildingMenuViewModel(
            BuildingEventChannel eventChannel,
            IResourceManager resourceManager,
            List<BuildingData> buildings)
        {
            this.eventChannel = eventChannel;
            this.resourceManager = resourceManager;
            this.allBuildings = buildings;

            selectedCategory = BuildingCategory.Residential;

            // Subscribe to resource changes
            eventChannel.Subscribe<ResourcesChangedEvent>(OnResourcesChanged);
        }

        // Commands
        public void OpenMenu()
        {
            IsMenuOpen = true;
        }

        public void CloseMenu()
        {
            IsMenuOpen = false;
        }

        public void SelectCategory(BuildingCategory category)
        {
            SelectedCategory = category;
        }

        public void SelectBuilding(BuildingData building)
        {
            SelectedBuilding = building;
        }

        public void StartPlacement()
        {
            if (SelectedBuilding != null && CanAffordSelected)
            {
                OnBuildingStartPlacement?.Invoke(SelectedBuilding);
                CloseMenu();
            }
        }

        // Event handlers
        private void OnResourcesChanged(ResourcesChangedEvent e)
        {
            OnPropertyChanged(nameof(CanAffordSelected));
            OnPropertyChanged(nameof(SelectedBuildingRequirements));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ResourceRequirementViewModel
    {
        public string ResourceId { get; set; }
        public int Required { get; set; }
        public int Available { get; set; }
        public bool HasEnough { get; set; }
    }