using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Items
{
    /// <summary>
    /// Менеджер для управления подбором предметов
    /// </summary>
    public class PickupManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float pickupRange = 1.5f;
        [SerializeField] private KeyCode pickupKey = KeyCode.E;
        [SerializeField] private bool showDebugInfo = true;

        private List<PickupItem> nearbyItems = new List<PickupItem>();
        private PickupItem currentClosestItem;
        private static PickupManager instance;

        public static PickupManager Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            UpdateNearbyItems();

            // Обрабатываем нажатие клавиши подбора
            if (Input.GetKeyDown(pickupKey))
            {
                TryPickupClosestItem();
            }
        }

        private void UpdateNearbyItems()
        {
            // Находим все предметы в радиусе
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupRange);

            nearbyItems.Clear();

            foreach (var collider in colliders)
            {
                PickupItem pickup = collider.GetComponent<PickupItem>();
                if (pickup != null && !pickup.IsBeingPickedUp && !pickup.AutoPickup)
                {
                    nearbyItems.Add(pickup);
                }
            }

            // Находим ближайший предмет
            if (nearbyItems.Count > 0)
            {
                PickupItem newClosest = nearbyItems
                    .OrderBy(item => Vector2.Distance(transform.position, item.transform.position))
                    .First();

                // Если ближайший предмет изменился
                if (newClosest != currentClosestItem)
                {
                    // Скрываем подсказку у старого
                    if (currentClosestItem != null)
                    {
                        currentClosestItem.SetHighlighted(false);
                    }

                    // Показываем у нового
                    currentClosestItem = newClosest;
                    currentClosestItem.SetHighlighted(true);
                }
            }
            else
            {
                // Нет предметов рядом
                if (currentClosestItem != null)
                {
                    currentClosestItem.SetHighlighted(false);
                    currentClosestItem = null;
                }
            }
        }

        private void TryPickupClosestItem()
        {
            if (currentClosestItem != null && !currentClosestItem.IsBeingPickedUp)
            {
                Debug.Log($"[PickupManager] Picking up: {currentClosestItem.name}");
                currentClosestItem.ForcePickup();
            }
        }

        // Метод для регистрации предмета (вызывается из PickupItem)
        public void RegisterPickupItem(PickupItem item)
        {
            // Можно использовать для дополнительной логики
        }

        public void UnregisterPickupItem(PickupItem item)
        {
            nearbyItems.Remove(item);
            if (currentClosestItem == item)
            {
                currentClosestItem = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugInfo) return;

            // Показываем радиус подбора
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRange);

            // Показываем линию к ближайшему предмету
            if (currentClosestItem != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentClosestItem.transform.position);
            }
        }
    }
}