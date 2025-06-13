using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Items
{
    /// <summary>
    /// Компонент для предметов, которые можно подобрать с земли
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PickupItem : MonoBehaviour
    {
        [Header("Item Settings")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity = 1;

        [Header("Pickup Settings")]
        [SerializeField] private float pickupRadius = 1f;
        [SerializeField] private bool autoPickup = false;
        [SerializeField] private float magnetSpeed = 5f;
        [SerializeField] private float magnetStartDistance = 2f;

        [Header("Hint Settings")]
        [SerializeField] private float hintDelay = 2f; // Задержка перед показом подсказки
        [SerializeField] private GameObject hintPrefab; // Префаб подсказки
        [SerializeField] private Vector3 hintOffset = new Vector3(0, 1f, 0); // Смещение подсказки

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float bobHeight = 0.1f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private bool rotateItem = false;
        [SerializeField] private float rotateSpeed = 90f;

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
        private bool isPlayerNear = false;
        private bool hintShown = false;

        private void Awake()
        {
            itemCollider = GetComponent<Collider2D>();
            itemCollider.isTrigger = true;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && itemData != null && itemData.icon != null)
            {
                spriteRenderer.sprite = itemData.icon;
            }

            originalY = transform.position.y;

            // Создаем префаб подсказки если не назначен
            if (hintPrefab == null)
            {
                CreateDefaultHintPrefab();
            }
        }

        private void CreateDefaultHintPrefab()
        {
            // Создаем простую подсказку если префаб не назначен
            GameObject hint = new GameObject("PickupHint");

            // Добавляем Canvas для мирового пространства
            Canvas canvas = hint.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingLayerName = "UI";

            // Добавляем CanvasScaler
            CanvasScaler scaler = hint.AddComponent<CanvasScaler>();

            // Создаем текст
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(hint.transform);

            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "Press [E] to pickup";
            text.fontSize = 2;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            // Настраиваем RectTransform
            RectTransform rectTransform = text.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(10, 2);
            rectTransform.localScale = Vector3.one * 0.1f;

            hintPrefab = hint;
            hint.SetActive(false);
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerInventory = player.GetComponentInChildren<Inventory>();
            }

            AnimateSpawn();
        }

        private void Update()
        {
            if (isBeingPickedUp) return;

            ApplyVisualEffects();

            // Обновляем таймер нахождения игрока рядом
            if (isPlayerNear && !autoPickup)
            {
                playerNearTimer += Time.deltaTime;

                // Показываем подсказку после задержки
                if (playerNearTimer >= hintDelay && !hintShown)
                {
                    ShowPickupHint();
                }
            }

            // Автоподбор
            if (autoPickup && playerTransform != null)
            {
                float distance = Vector2.Distance(transform.position, playerTransform.position);

                if (distance <= magnetStartDistance)
                {
                    Vector3 direction = (playerTransform.position - transform.position).normalized;
                    transform.position += direction * magnetSpeed * Time.deltaTime;

                    if (distance <= pickupRadius)
                    {
                        TryPickup();
                    }
                }
            }
        }

        private void ApplyVisualEffects()
        {
            if (bobHeight > 0)
            {
                float newY = originalY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }

            if (rotateItem)
            {
                transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
            }
        }

        private void AnimateSpawn()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!autoPickup && other.CompareTag("Player"))
            {
                isPlayerNear = true;
                playerNearTimer = 0f;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!autoPickup && other.CompareTag("Player"))
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    TryPickup();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!autoPickup && other.CompareTag("Player"))
            {
                isPlayerNear = false;
                playerNearTimer = 0f;
                HidePickupHint();
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

        private void ShowPickupHint()
        {
            if (hintInstance == null && hintPrefab != null)
            {
                // Создаем подсказку
                hintInstance = Instantiate(hintPrefab, transform.position + hintOffset, Quaternion.identity);
                hintInstance.transform.SetParent(transform);

                // Анимация появления
                CanvasGroup canvasGroup = hintInstance.GetComponentInChildren<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = hintInstance.AddComponent<CanvasGroup>();
                }

                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1f, 0.3f);

                // Анимация покачивания
                Transform hintTransform = hintInstance.transform;
                hintTransform.localScale = Vector3.one * 0.8f;
                hintTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

                // Легкое покачивание вверх-вниз
                hintTransform.DOLocalMoveY(hintOffset.y + 0.1f, 1f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);

                hintShown = true;
            }
        }

        private void HidePickupHint()
        {
            if (hintInstance != null)
            {
                // Анимация исчезновения
                CanvasGroup canvasGroup = hintInstance.GetComponentInChildren<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0f, 0.2f).OnComplete(() => {
                        Destroy(hintInstance);
                        hintInstance = null;
                    });
                }
                else
                {
                    Destroy(hintInstance);
                    hintInstance = null;
                }

                hintShown = false;
            }
        }

        public void SetItem(ItemData item, int amount = 1)
        {
            itemData = item;
            quantity = amount;

            if (spriteRenderer != null && item != null && item.icon != null)
            {
                spriteRenderer.sprite = item.icon;
            }
        }

        public void ForcePickup()
        {
            TryPickup();
        }

        private void OnDestroy()
        {
            // Убираем подсказку при уничтожении
            if (hintInstance != null)
            {
                Destroy(hintInstance);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);

            if (autoPickup)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, magnetStartDistance);
            }

            // Показываем позицию подсказки
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + hintOffset, new Vector3(1, 0.2f, 0));
        }
    }
}