using UnityEngine;

namespace Items
{
    /// <summary>
    /// Спавнер предметов для тестирования и дропа
    /// </summary>
    public class ItemSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject pickupItemPrefab;
        [SerializeField] private ItemData[] itemsToSpawn;
        [SerializeField] private int minQuantity = 1;
        [SerializeField] private int maxQuantity = 3;

        [Header("Spawn Area")]
        [SerializeField] private float spawnRadius = 1f;
        [SerializeField] private float spawnForce = 3f;
        [SerializeField] private bool addRandomRotation = true;

        /// <summary>
        /// Спавнит конкретный предмет
        /// </summary>
        public void SpawnItem(ItemData itemData, int quantity = 1, Vector3? position = null)
        {
            if (itemData == null || pickupItemPrefab == null) return;

            Vector3 spawnPos = position ?? transform.position;

            // Добавляем случайное смещение
            if (spawnRadius > 0)
            {
                Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                spawnPos += new Vector3(randomOffset.x, randomOffset.y, 0);
            }

            // Создаем объект
            GameObject pickupGO = Instantiate(pickupItemPrefab, spawnPos, Quaternion.identity);

            // Настраиваем компонент
            PickupItem pickup = pickupGO.GetComponent<PickupItem>();
            if (pickup != null)
            {
                pickup.SetItem(itemData, quantity);
            }

            // Добавляем физику для разброса
            Rigidbody2D rb = pickupGO.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = pickupGO.AddComponent<Rigidbody2D>();
            }

            // Применяем силу для разброса
            if (spawnForce > 0)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                rb.AddForce(randomDirection * spawnForce, ForceMode2D.Impulse);

                if (addRandomRotation)
                {
                    rb.AddTorque(Random.Range(-180f, 180f));
                }
            }

            // Убираем физику через время
            Destroy(rb, 1f);
        }

        /// <summary>
        /// Спавнит случайный предмет из списка
        /// </summary>
        public void SpawnRandomItem()
        {
            if (itemsToSpawn.Length == 0) return;

            ItemData randomItem = itemsToSpawn[Random.Range(0, itemsToSpawn.Length)];
            int randomQuantity = Random.Range(minQuantity, maxQuantity + 1);

            SpawnItem(randomItem, randomQuantity);
        }

        /// <summary>
        /// Спавнит несколько предметов
        /// </summary>
        public void SpawnMultipleItems(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnRandomItem();
            }
        }

        // Для тестирования
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                SpawnRandomItem();
                Debug.Log("Spawned random item");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}