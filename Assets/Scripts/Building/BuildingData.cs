using UnityEngine;
using System.Collections.Generic;

namespace Building
{
    /// <summary>
    /// Данные о здании - ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Building", menuName = "Building System/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [Header("Basic Info")]
        public string buildingName = "New Building";
        public string description = "Building description";
        public Sprite icon;
        public GameObject prefab; // Префаб здания

        [Header("Size")]
        public Vector2Int size = new Vector2Int(2, 2); // Размер в тайлах

        [Header("Requirements")]
        public int requiredLevel = 1;
        public List<ResourceRequirement> resourceRequirements = new List<ResourceRequirement>();
        public List<string> requiredQuests = new List<string>(); // ID квестов

        [Header("Building Properties")]
        public BuildingCategory category = BuildingCategory.Production;
        public float buildTime = 5f; // Время строительства в секундах
        public bool canRotate = true;

        [Header("Visual")]
        public GameObject ghostPrefab; // Префаб для предпросмотра (если отличается)
        public Color validPlacementColor = new Color(0, 1, 0, 0.5f);
        public Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);

        /// <summary>
        /// Проверяет, разблокировано ли здание для игрока
        /// </summary>
        public bool IsUnlocked(int playerLevel)
        {
            if (playerLevel < requiredLevel)
                return false;

            // TODO: Проверка квестов
            foreach (var questId in requiredQuests)
            {
                // if (!QuestManager.Instance.IsQuestCompleted(questId))
                //     return false;
            }

            return true;
        }

      
        public bool CanAfford(Inventory playerInventory)
        {
            foreach (var requirement in resourceRequirements)
            {
                // Проверяем количество ресурсов в инвентаре
                int playerAmount = GetItemCount(playerInventory, requirement.resource);
                if (playerAmount < requirement.amount)
                    return false;
            }

            return true;
        }

        private int GetItemCount(Inventory inventory, ItemData resource)
        {
            int count = 0;
            foreach (var item in inventory.items)
            {
                if (item.itemData == resource)
                    count += item.quantity;
            }
            return count;
        }
    }

    /// <summary>
    /// Требование ресурса для постройки
    /// </summary>
    [System.Serializable]
    public class ResourceRequirement
    {
        public ItemData resource; // Ресурс (дерево, камень и т.д.)
        public int amount; // Количество

        public ResourceRequirement(ItemData res, int amt)
        {
            resource = res;
            amount = amt;
        }
    }

    /// <summary>
    /// Категории зданий
    /// </summary>
    public enum BuildingCategory
    {
        Storage,      // Хранилища
        Production,   // Производство
        Crafting,     // Ремесло
        Decoration,   // Декорации
        Defense,      // Защита
        Special       // Особые
    }
}