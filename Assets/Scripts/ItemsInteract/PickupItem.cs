using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Items
{
    [RequireComponent(typeof(Collider2D))]
    public class PickupItem : MonoBehaviour
    {
        [Header("Item Settings")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity = 1;

        [Header("Pickup Settings")]
        [SerializeField] private float pickupRadius = 2f;
        [SerializeField] private bool forceAutoPickup = false; // Принудительно включить автоподбор
        [SerializeField] private bool forceManualPickup = false; // Принудительно выключить автоподбор
        [SerializeField] private float magnetSpeed = 5f;
        [SerializeField] private float magnetStartDistance = 2f;

        [Header("Auto-Pickup by Type")]
        [Tooltip("Типы предметов, которые подбираются автоматически")]
        [SerializeField]
        private ItemType[] autoPickupTypes = new ItemType[]
        {
            ItemType.Resource,
            ItemType.Consumable,
            ItemType.Misc
        };

        [Header("Hint Settings")]
        [SerializeField] private float hintDelay = 0.5f;
        [SerializeField] private GameObject hintPrefab;
        [SerializeField] private Vector3 hintOffset = new Vector3(0, 1f, 0);

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float bobHeight = 0.1f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private bool rotateItem = false;
        [SerializeField] private float rotateSpeed = 90f;

        [Header("Highlight")]
        [SerializeField] private Color highlightColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        [SerializeField] private float highlightScale = 1.1f;

        [Header("Sound")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private float pickupVolume = 1f;

        [Header("Effects")]
        [SerializeField] private GameObject pickupEffectPrefab;
        [SerializeField] private bool destroyOnPickup = true;

        private Transform playerTransform;
        private Inventory playerInventory;
        private bool isBeingPickedUp = false;
        private float originalY;
        private Collider2D itemCollider;

        // Для подсказки
        private GameObject hintInstance;
        private float playerNearTimer = 0f;
        private bool isHighlighted = false;
        private bool hintShown = false;
        private Color originalColor;
        private Vector3 originalScale;

        public bool IsBeingPickedUp => isBeingPickedUp;

        // Свойство для определения автоподбора
        public bool AutoPickup
        {
            get
            {
                // Принудительные настройки имеют приоритет
                if (forceAutoPickup) return true;
                if (forceManualPickup) return false;

                // Проверяем по типу предмета
                if (itemData != null)
                {
                    foreach (var type in autoPickupTypes)
                    {
                        if (itemData.ItemType == type)
                            return true;
                    }
                }

                return false;
            }
        }

        private void Awake()
        {
            itemCollider = GetComponent<Collider2D>();
            itemCollider.isTrigger = true;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
                originalScale = transform.localScale;
            }

            originalY = transform.position.y;
        }

        private void Start()
        {
            // Устанавливаем спрайт из ItemData
            if (itemData != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = itemData.icon;
            }

            // Находим игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerInventory = player.GetComponent<Inventory>();
            }

            // Начальная анимация появления
            transform.localScale = Vector3.zero;
            transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack);
        }

        private void Update()
        {
            if (playerTransform == null || isBeingPickedUp) return;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // Визуальные эффекты
            UpdateVisualEffects();

            // Магнит и автоподбор
            if (AutoPickup && distanceToPlayer <= magnetStartDistance)
            {
                // Двигаемся к игроку
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    playerTransform.position,
                    magnetSpeed * Time.deltaTime
                );

                // Автоподбор при приближении
                if (distanceToPlayer <= 0.5f)
                {
                    TryPickup();
                }
            }
            else if (!AutoPickup)
            {
                // Обработка подсказки для ручного подбора
                HandlePickupHint(distanceToPlayer);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var inventory = other.GetComponent<Inventory>();
            if (inventory != null)
            {
                playerInventory = inventory;
                playerTransform = other.transform;

                // Показываем подсказку только для предметов с ручным подбором
                if (!AutoPickup)
                {
                    ShowPickupHint();
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player") || AutoPickup) return;

            // Ручной подбор на E
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryPickup();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            // Убираем подсветку
            if (isHighlighted)
            {
                SetHighlighted(false);
            }

            // Скрываем подсказку
            HidePickupHint();
            playerNearTimer = 0f;
        }

        private void UpdateVisualEffects()
        {
            // Покачивание
            if (bobHeight > 0)
            {
                float newY = originalY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }

            // Вращение
            if (rotateItem)
            {
                transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
            }
        }

        private void HandlePickupHint(float distanceToPlayer)
        {
            if (distanceToPlayer <= pickupRadius)
            {
                // Подсветка предмета
                if (!isHighlighted)
                {
                    SetHighlighted(true);
                }

                // Таймер для показа подсказки
                playerNearTimer += Time.deltaTime;
                if (playerNearTimer >= hintDelay && !hintShown)
                {
                    ShowPickupHint();
                    hintShown = true;
                }
            }
            else
            {
                // Убираем подсветку
                if (isHighlighted)
                {
                    SetHighlighted(false);
                }

                // Скрываем подсказку
                HidePickupHint();
                playerNearTimer = 0f;
                hintShown = false;
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            isHighlighted = highlighted;

            if (spriteRenderer != null)
            {
                if (highlighted)
                {
                    spriteRenderer.color = highlightColor;
                    transform.DOScale(originalScale * highlightScale, 0.2f);
                }
                else
                {
                    spriteRenderer.color = originalColor;
                    transform.DOScale(originalScale, 0.2f);
                }
            }
        }

        private void ShowPickupHint()
        {
            if (hintInstance == null && hintPrefab != null)
            {
                hintInstance = Instantiate(hintPrefab, transform.position + hintOffset, Quaternion.identity);
                hintInstance.transform.SetParent(transform);

                // Настраиваем текст подсказки
                var textComponent = hintInstance.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    string itemName = itemData != null ? itemData.itemName : "Item";
                    textComponent.text = $"[E] Pick up {itemName}";
                }

                // Анимация появления
                CanvasGroup canvasGroup = hintInstance.GetComponentInChildren<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = hintInstance.AddComponent<CanvasGroup>();
                }

                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1f, 0.3f);
            }
        }

        private void HidePickupHint()
        {
            if (hintInstance != null)
            {
                Destroy(hintInstance);
                hintInstance = null;
                hintShown = false;
            }
        }

        private void TryPickup()
        {
            if (isBeingPickedUp || playerInventory == null || itemData == null) return;

            isBeingPickedUp = true;

            playerInventory.AddItem(itemData, quantity);

            PlayPickupEffects();

            // Скрываем подсказку
            HidePickupHint();

            if (destroyOnPickup)
            {
                transform.DOScale(Vector3.zero, 0.2f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => Destroy(gameObject));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        // Публичный метод для принудительного подбора
        public void ForcePickup()
        {
            TryPickup();
        }

        private void PlayPickupEffects()
        {
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
            }

            if (pickupEffectPrefab != null)
            {
                GameObject effect = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }

        public void SetItem(ItemData data, int amount = 1)
        {
            itemData = data;
            quantity = amount;

            if (spriteRenderer != null && data != null)
            {
                spriteRenderer.sprite = data.icon;
            }
        }

        private void OnDestroy()
        {
            // Убираем подсказку
            HidePickupHint();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, magnetStartDistance);
        }
    }
}