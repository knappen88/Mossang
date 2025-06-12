namespace Combat.Equipment
{
    using UnityEngine;
    using UnityEngine.Events;

    [RequireComponent(typeof(WeaponEquipmentController))]
    public class WeaponCombatHandler : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float defaultAttackRange = 1.5f;

        [Header("Events")]
        public UnityEvent<float> OnAttackStarted; // Передает длительность атаки
        public UnityEvent OnAttackHit;
        public UnityEvent OnAttackMissed;
        public UnityEvent OnAttackFinished;

        private WeaponEquipmentController equipmentController;
        private PlayerMovement playerMovement;
        private PlayerAnimator playerAnimator;

        private bool isAttacking;
        private float nextAttackTime;

        private void Awake()
        {
            equipmentController = GetComponent<WeaponEquipmentController>();
            playerMovement = GetComponent<PlayerMovement>();
            playerAnimator = GetComponent<PlayerAnimator>();
        }

        private void Update()
        {
            HandleCombatInput();
        }

        private void HandleCombatInput()
        {
            if (Input.GetMouseButtonDown(0) && CanAttack())
            {
                StartAttack();
            }
        }

        private bool CanAttack()
        {
            return !isAttacking &&
                   Time.time >= nextAttackTime &&
                   equipmentController.HasWeaponEquipped();
        }

        private void StartAttack()
        {
            var weapon = equipmentController.GetCurrentWeapon();
            if (weapon == null) return;

            isAttacking = true;

            // Замораживаем движение во время атаки
            if (playerMovement != null)
            {
                playerMovement.DisableMovement();
            }

            // Замораживаем направление анимации
            if (playerAnimator != null)
            {
                playerAnimator.FreezeDirection();
            }

            // Запускаем анимацию атаки
            equipmentController.TriggerAttack();

            // Рассчитываем время следующей атаки
            float attackDuration = weapon.animationSet != null ?
                weapon.animationSet.attackDuration : 1f;

            nextAttackTime = Time.time + (1f / weapon.attackSpeed);

            OnAttackStarted?.Invoke(attackDuration);

            // Запускаем корутину для обработки атаки
            StartCoroutine(AttackSequence(weapon, attackDuration));
        }

        private System.Collections.IEnumerator AttackSequence(WeaponData weapon, float duration)
        {
            // Ждем до момента нанесения урона
            float hitTime = weapon.animationSet != null ?
                weapon.animationSet.attackHitTimeNormalized : duration * 0.5f;

            yield return new WaitForSeconds(hitTime);

            // Выполняем проверку попадания
            PerformAttack(weapon);

            // Ждем окончания анимации
            yield return new WaitForSeconds(duration - hitTime);

            // Завершаем атаку
            FinishAttack();
        }

        private void PerformAttack(WeaponData weapon)
        {
            // Определяем точку атаки
            Vector3 attackPos = attackPoint != null ?
                attackPoint.position : transform.position + transform.forward;

            // Проверяем попадания в радиусе
            Collider[] hits = Physics.OverlapSphere(
                attackPos,
                weapon.attackRange,
                weapon.targetLayers
            );

            bool hitSomething = false;

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                // Проверяем интерфейс получения урона
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // Наносим урон
                    damageable.TakeDamage(weapon.damage, transform.position);

                    // Отталкивание
                    var rb = hit.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 knockback = (hit.transform.position - transform.position).normalized;
                        rb.AddForce(knockback * weapon.knockbackForce, ForceMode.Impulse);
                    }

                    hitSomething = true;
                }
            }

            if (hitSomething)
            {
                OnAttackHit?.Invoke();
                PlayHitSound(weapon);
            }
            else
            {
                OnAttackMissed?.Invoke();
            }
        }

        private void FinishAttack()
        {
            isAttacking = false;

            // Возвращаем управление движением
            if (playerMovement != null)
            {
                playerMovement.EnableMovement();
            }

            // Размораживаем направление анимации
            if (playerAnimator != null)
            {
                playerAnimator.UnfreezeDirection();
            }

            OnAttackFinished?.Invoke();
        }

        private void PlayHitSound(WeaponData weapon)
        {
            if (weapon.hitSounds != null && weapon.hitSounds.Length > 0)
            {
                var audioSource = GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    var randomSound = weapon.hitSounds[Random.Range(0, weapon.hitSounds.Length)];
                    audioSource.PlayOneShot(randomSound);
                }
            }
        }

        // Визуализация для отладки
        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                var weapon = equipmentController?.GetCurrentWeapon();
                float range = weapon != null ? weapon.attackRange : defaultAttackRange;

                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attackPoint.position, range);
            }
        }
    }
}