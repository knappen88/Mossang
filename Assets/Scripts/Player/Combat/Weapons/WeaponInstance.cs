namespace Combat.Weapons
{
    using UnityEngine;
    using Combat.Core;
    using System.Collections.Generic;

    [RequireComponent(typeof(Animator))]
    public class WeaponInstance : MonoBehaviour, IEquippable
    {
        [Header("Weapon Configuration")]
        [SerializeField] private WeaponType weaponType;
        [SerializeField] private Transform gripPoint;

        [Header("Sprites")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite attackSprite; // Опционально - другой спрайт при атаке

        [Header("Combat Colliders")]
        [SerializeField] private Collider2D damageCollider;
        [SerializeField] private float colliderEnableDelay = 0.1f;
        [SerializeField] private float colliderDisableDelay = 0.4f;

        [Header("Sorting")]
        [SerializeField] private int sortingOrderOffset = 1; // Относительно персонажа
        [SerializeField] private bool adjustSortingByDirection = true;

        [Header("Visual Effects")]
        [SerializeField] private GameObject slashEffectPrefab; // 2D эффект взмаха
        [SerializeField] private ParticleSystem hitEffectPrefab;
        [SerializeField] private float effectScale = 1f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] swingSounds;
        [SerializeField] private AudioClip[] hitSounds;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        private GameObject owner;
        private WeaponData weaponData;
        private SpriteRenderer spriteRenderer;
        private SpriteRenderer ownerRenderer;
        private bool isAttacking;
        private List<GameObject> hitTargets = new List<GameObject>();
        private int baseOwnerSortingOrder;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            SetupComponents();

            if (damageCollider != null)
                damageCollider.enabled = false;
        }

        private void SetupComponents()
        {
            // Создаем AudioSource если нет
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = 0.7f;
            }

            // Настройка 2D коллайдера
            if (damageCollider != null)
            {
                damageCollider.isTrigger = true;

                // Добавляем Rigidbody2D если нужно
                var rb2d = damageCollider.GetComponent<Rigidbody2D>();
                if (rb2d == null)
                {
                    rb2d = damageCollider.gameObject.AddComponent<Rigidbody2D>();
                    rb2d.bodyType = RigidbodyType2D.Kinematic;
                }
            }

            // Настройка спрайта
            if (spriteRenderer != null && idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
        }

        public void OnEquip(GameObject newOwner)
        {
            owner = newOwner;

            // Получаем WeaponData
            var equipController = owner.GetComponent<Combat.Equipment.WeaponEquipmentController>();
            if (equipController != null)
            {
                weaponData = equipController.GetCurrentWeapon();
            }

            // Получаем SpriteRenderer владельца для сортировки
            ownerRenderer = owner.GetComponentInChildren<SpriteRenderer>();
            if (ownerRenderer != null)
            {
                baseOwnerSortingOrder = ownerRenderer.sortingOrder;
                UpdateSortingOrder();
            }

            // Настройка коллизий
            if (damageCollider != null && owner != null)
            {
                var ownerColliders = owner.GetComponentsInChildren<Collider2D>();
                foreach (var col in ownerColliders)
                {
                    Physics2D.IgnoreCollision(damageCollider, col, true);
                }
            }

            Debug.Log($"[WeaponInstance2D] {name} equipped by {owner.name}");
        }

        public void OnUnequip()
        {
            StopAttack();
            owner = null;
            weaponData = null;
            ownerRenderer = null;
        }

        public Transform GetEquipTransform()
        {
            return gripPoint ?? transform;
        }

        private void Update()
        {
            if (owner != null && adjustSortingByDirection)
            {
                UpdateSortingOrder();
            }
        }

        private void UpdateSortingOrder()
        {
            if (spriteRenderer == null || ownerRenderer == null) return;

            // Получаем направление от PlayerAnimator
            var playerAnimator = owner.GetComponent<PlayerAnimator>();
            if (playerAnimator != null)
            {
                int direction = playerAnimator.GetCurrentDirection();

                // Меняем порядок сортировки в зависимости от направления
                // 0 = Front, 1 = Back, 2 = Side
                if (direction == 1) // Back - оружие за персонажем
                {
                    spriteRenderer.sortingOrder = ownerRenderer.sortingOrder - sortingOrderOffset;
                }
                else // Front или Side - оружие перед персонажем
                {
                    spriteRenderer.sortingOrder = ownerRenderer.sortingOrder + sortingOrderOffset;
                }

                // Для боковой атаки можем отразить спрайт
                if (direction == 2 && ownerRenderer != null)
                {
                    spriteRenderer.flipX = ownerRenderer.flipX;
                }
            }
        }

        // Animation Event Methods
        public void OnAttackStart()
        {
            StartAttack();
        }

        public void OnAttackHit()
        {
            // Создаем 2D эффект взмаха
            if (slashEffectPrefab != null)
            {
                CreateSlashEffect();
            }
        }

        public void OnAttackEnd()
        {
            StopAttack();
        }

        public void StartAttack()
        {
            if (isAttacking) return;

            isAttacking = true;
            hitTargets.Clear();

            // Меняем спрайт если есть
            if (attackSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = attackSprite;
            }

            // Звук взмаха
            PlaySwingSound();

            // Включаем коллайдер с задержкой
            if (damageCollider != null)
                Invoke(nameof(EnableDamageCollider), colliderEnableDelay);
        }

        public void StopAttack()
        {
            isAttacking = false;

            // Возвращаем обычный спрайт
            if (idleSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = idleSprite;
            }

            // Выключаем коллайдер
            if (damageCollider != null)
            {
                Invoke(nameof(DisableDamageCollider), colliderDisableDelay);
            }
        }

        private void EnableDamageCollider()
        {
            if (damageCollider != null)
                damageCollider.enabled = true;
        }

        private void DisableDamageCollider()
        {
            if (damageCollider != null)
                damageCollider.enabled = false;
        }

        private void CreateSlashEffect()
        {
            if (slashEffectPrefab == null || owner == null) return;

            // Создаем эффект в позиции оружия
            GameObject effect = Instantiate(slashEffectPrefab, transform.position, transform.rotation);
            effect.transform.localScale = Vector3.one * effectScale;

            // Настраиваем сортировку
            var effectRenderer = effect.GetComponent<SpriteRenderer>();
            if (effectRenderer != null && spriteRenderer != null)
            {
                effectRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }

            // Поворачиваем эффект в зависимости от направления атаки
            var playerAnimator = owner.GetComponent<PlayerAnimator>();
            if (playerAnimator != null)
            {
                int direction = playerAnimator.GetCurrentDirection();

                switch (direction)
                {
                    case 0: // Front
                        effect.transform.rotation = Quaternion.Euler(0, 0, -90);
                        break;
                    case 1: // Back
                        effect.transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                    case 2: // Side
                        bool flipX = ownerRenderer != null && ownerRenderer.flipX;
                        effect.transform.rotation = Quaternion.Euler(0, 0, flipX ? 180 : 0);
                        if (effectRenderer != null)
                            effectRenderer.flipY = flipX;
                        break;
                }
            }

            Destroy(effect, 0.5f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isAttacking || other.gameObject == owner) return;

            // Проверяем, что не били этот объект в этой атаке
            if (hitTargets.Contains(other.gameObject)) return;

            // Проверяем слои
            if (weaponData != null && (weaponData.targetLayers.value & (1 << other.gameObject.layer)) == 0)
                return;

            // Добавляем в список пораженных
            hitTargets.Add(other.gameObject);

            // Проверяем IDamageable
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null && weaponData != null)
            {
                damageable.TakeDamage(weaponData.damage, transform.position);

                // Эффект попадания
                SpawnHitEffect(other.transform.position);

                // Звук попадания
                PlayHitSound();

                // Отдача
                ApplyKnockback2D(other.gameObject);
            }
        }

        private void SpawnHitEffect(Vector3 position)
        {
            if (hitEffectPrefab != null)
            {
                var effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);

                // Настраиваем сортировку для 2D
                var effectRenderer = effect.GetComponent<ParticleSystemRenderer>();
                if (effectRenderer != null && spriteRenderer != null)
                {
                    effectRenderer.sortingOrder = spriteRenderer.sortingOrder + 10;
                }

                Destroy(effect.gameObject, 2f);
            }
        }

        private void ApplyKnockback2D(GameObject target)
        {
            if (weaponData == null || weaponData.knockbackForce <= 0) return;

            var rb2d = target.GetComponent<Rigidbody2D>();
            if (rb2d != null && rb2d.bodyType != RigidbodyType2D.Static)
            {
                Vector2 knockbackDir = (target.transform.position - owner.transform.position).normalized;
                rb2d.AddForce(knockbackDir * weaponData.knockbackForce, ForceMode2D.Impulse);
            }
        }

        private void PlaySwingSound()
        {
            if (audioSource != null && swingSounds.Length > 0)
            {
                var sound = swingSounds[Random.Range(0, swingSounds.Length)];
                audioSource.PlayOneShot(sound);
            }
        }

        private void PlayHitSound()
        {
            if (audioSource != null)
            {
                AudioClip[] sounds = hitSounds.Length > 0 ? hitSounds :
                    (weaponData?.hitSounds ?? new AudioClip[0]);

                if (sounds.Length > 0)
                {
                    var sound = sounds[Random.Range(0, sounds.Length)];
                    audioSource.PlayOneShot(sound);
                }
            }
        }

        // Визуализация для отладки
        private void OnDrawGizmosSelected()
        {
            if (!showDebugInfo) return;

            // Показываем grip point
            if (gripPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(gripPoint.position, 0.05f);
            }

            // Показываем damage collider для 2D
            if (damageCollider != null)
            {
                Gizmos.color = isAttacking ? Color.red : Color.yellow;

                if (damageCollider is BoxCollider2D box)
                {
                    Vector3 center = damageCollider.transform.TransformPoint(box.offset);
                    Vector3 size = new Vector3(box.size.x, box.size.y, 0.1f);

                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Gizmos.matrix = Matrix4x4.TRS(center, damageCollider.transform.rotation, damageCollider.transform.lossyScale);
                    Gizmos.DrawWireCube(Vector3.zero, size);
                    Gizmos.matrix = oldMatrix;
                }
                else if (damageCollider is CircleCollider2D circle)
                {
                    Vector3 center = damageCollider.transform.TransformPoint(circle.offset);
                    Gizmos.DrawWireSphere(center, circle.radius * damageCollider.transform.lossyScale.x);
                }
            }
        }
    }
}