using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Items;

namespace Enemies.Core
{
    /// <summary>
    /// Базовый класс для всех врагов в игре
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("Base Stats")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float damage = 10f;
        [SerializeField] protected float attackRange = 1.5f;
        [SerializeField] protected float detectionRange = 5f;
        [SerializeField] protected float attackCooldown = 1f;

        [Header("Layers")]
        [SerializeField] protected LayerMask targetLayers;
        [SerializeField] protected LayerMask obstacleLayer;

        [Header("References")]
        [SerializeField] protected Animator animator;
        [SerializeField] protected SpriteRenderer spriteRenderer;

        [Header("Effects")]
        [SerializeField] protected GameObject hitEffectPrefab;
        [SerializeField] protected GameObject deathEffectPrefab;

        [Header("Loot")]
        [SerializeField] protected ItemData[] lootItems;
        [SerializeField] protected int minLootCount = 0;
        [SerializeField] protected int maxLootCount = 2;
        [SerializeField] protected float lootDropRadius = 1f;

        // Состояния
        protected float currentHealth;
        protected bool isDead = false;
        protected bool isAttacking = false;
        protected float lastAttackTime;

        // Компоненты
        protected Rigidbody2D rb;
        protected Collider2D col;
        protected Transform target;

        // События
        public System.Action<float> OnHealthChanged;
        public System.Action OnDeath;
        public System.Action<Transform> OnTargetDetected;
        public System.Action OnTargetLost;

        // IDamageable implementation
        public bool IsAlive => !isDead;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();

            if (animator == null)
                animator = GetComponent<Animator>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            currentHealth = maxHealth;
        }

        protected virtual void Start()
        {
            InitializeEnemy();
        }

        protected virtual void Update()
        {
            if (isDead) return;

            // Обновляем поведение врага
            UpdateBehavior();

            // Проверяем цель
            CheckForTarget();

            // Обновляем анимации
            UpdateAnimations();
        }

        protected virtual void FixedUpdate()
        {
            if (isDead) return;

            // Физическое движение
            HandleMovement();
        }

        /// <summary>
        /// Инициализация специфичная для типа врага
        /// </summary>
        protected abstract void InitializeEnemy();

        /// <summary>
        /// Обновление поведения врага (AI логика)
        /// </summary>
        protected abstract void UpdateBehavior();

        /// <summary>
        /// Обработка движения врага
        /// </summary>
        protected abstract void HandleMovement();

        /// <summary>
        /// Проверка наличия цели в радиусе обнаружения
        /// </summary>
        protected virtual void CheckForTarget()
        {
            if (target == null)
            {
                // Ищем игрока в радиусе
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, targetLayers);

                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Player"))
                    {
                        target = hit.transform;
                        OnTargetDetected?.Invoke(target);
                        break;
                    }
                }
            }
            else
            {
                // Проверяем расстояние до цели
                float distance = Vector2.Distance(transform.position, target.position);

                if (distance > detectionRange * 1.5f) // Даем немного больше расстояния перед потерей
                {
                    target = null;
                    OnTargetLost?.Invoke();
                }
            }
        }

        /// <summary>
        /// Атака цели
        /// </summary>
        protected virtual void AttackTarget()
        {
            if (target == null || isAttacking || Time.time - lastAttackTime < attackCooldown)
                return;

            float distance = Vector2.Distance(transform.position, target.position);

            if (distance <= attackRange)
            {
                isAttacking = true;
                lastAttackTime = Time.time;

                // Запускаем анимацию атаки
                animator?.SetTrigger("Attack");

                // Наносим урон (можно вызвать через Animation Event)
                StartCoroutine(PerformAttack());
            }
        }

        protected virtual IEnumerator PerformAttack()
        {
            yield return new WaitForSeconds(0.5f); // Задержка до момента удара в анимации

            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.position);

                if (distance <= attackRange)
                {
                    var damageable = target.GetComponent<IDamageable>();
                    damageable?.TakeDamage(damage, transform.position);
                }
            }

            isAttacking = false;
        }

        /// <summary>
        /// Получение урона (IDamageable)
        /// </summary>
        public virtual void TakeDamage(float damageAmount, Vector3 damageSource)
        {
            if (isDead) return;

            currentHealth -= damageAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            OnHealthChanged?.Invoke(currentHealth / maxHealth);

            // Визуальная обратная связь
            ShowHitFeedback(damageSource);

            // Проверяем смерть
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Агрессия на атакующего
                GameObject attacker = GetAttackerFromSource(damageSource);
                if (attacker != null && attacker.CompareTag("Player"))
                {
                    target = attacker.transform;
                    OnTargetDetected?.Invoke(target);
                }
            }
        }

        protected virtual GameObject GetAttackerFromSource(Vector3 source)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(source, 0.5f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                    return hit.gameObject;
            }
            return null;
        }

        /// <summary>
        /// Смерть врага
        /// </summary>
        protected virtual void Die()
        {
            if (isDead) return;

            isDead = true;

            // Отключаем компоненты
            rb.velocity = Vector2.zero;
            rb.simulated = false;
            col.enabled = false;

            // Анимация смерти
            animator?.SetTrigger("Die");

            // Эффект смерти
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // Спавним лут
            SpawnLoot();

            // Событие смерти
            OnDeath?.Invoke();

            // Уничтожаем объект
            StartCoroutine(DestroyAfterDelay(2f));
        }

        protected virtual void SpawnLoot()
        {
            if (lootItems == null || lootItems.Length == 0) return;

            int lootCount = Random.Range(minLootCount, maxLootCount + 1);

            for (int i = 0; i < lootCount; i++)
            {
                if (lootItems.Length > 0)
                {
                    ItemData randomItem = lootItems[Random.Range(0, lootItems.Length)];
                    Vector2 randomOffset = Random.insideUnitCircle * lootDropRadius;
                    Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

                    // Используем ItemSpawner если есть
                    var spawner = GetComponent<ItemSpawner>();
                    if (spawner != null)
                    {
                        spawner.SpawnItem(randomItem, 1, spawnPos);
                    }
                }
            }
        }

        protected virtual void ShowHitFeedback(Vector3 damageSource)
        {
            // Эффект попадания
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            // Отталкивание
            Vector2 knockback = ((Vector2)transform.position - (Vector2)damageSource).normalized * 2f;
            rb.AddForce(knockback, ForceMode2D.Impulse);

            // Мигание спрайта
            StartCoroutine(FlashSprite());
        }

        protected IEnumerator FlashSprite()
        {
            if (spriteRenderer != null)
            {
                Color originalColor = spriteRenderer.color;
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
            }
        }

        protected IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }

        /// <summary>
        /// Обновление анимаций
        /// </summary>
        protected virtual void UpdateAnimations()
        {
            if (animator != null && rb != null)
            {
                animator.SetFloat("Speed", rb.velocity.magnitude);
                animator.SetBool("IsAttacking", isAttacking);

                // Поворот спрайта
                if (rb.velocity.x != 0 && spriteRenderer != null)
                {
                    spriteRenderer.flipX = rb.velocity.x < 0;
                }
            }
        }

        /// <summary>
        /// Проверка прямой видимости до цели
        /// </summary>
        protected bool HasLineOfSight(Transform target)
        {
            if (target == null) return false;

            Vector2 direction = target.position - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, direction.magnitude, obstacleLayer);

            return hit.collider == null;
        }

        /// <summary>
        /// Отладочная визуализация
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            // Радиус обнаружения
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Радиус атаки
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Линия до цели
            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
            }
        }
    }
}