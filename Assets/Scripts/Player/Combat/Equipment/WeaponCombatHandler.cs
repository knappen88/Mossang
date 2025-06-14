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
            Vector2 attackPos = attackPoint != null ?
                (Vector2)attackPoint.position : (Vector2)transform.position + Vector2.right * transform.localScale.x;

            // Проверяем попадания в радиусе (для 2D)
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                attackPos,
                weapon.attackRange,
                weapon.targetLayers
            );

            bool hitSomething = false;

            // Получаем EquipmentController один раз
            var equipment = GetComponent<Player.Equipment.EquipmentController>();
            ToolData currentTool = null;

            if (equipment != null)
            {
                currentTool = equipment.GetCurrentItem() as ToolData;
            }

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                // Проверяем интерфейс получения урона (для врагов)
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // Наносим урон
                    damageable.TakeDamage(weapon.damage, transform.position);

                    // Отталкивание для 2D
                    var rb = hit.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 knockback = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                        rb.AddForce(knockback * weapon.knockbackForce, ForceMode2D.Impulse);
                    }

                    hitSomething = true;
                    continue; // Переходим к следующему объекту
                }

                // Проверяем интерфейс IHarvestable (для ресурсов)
                var harvestable = hit.GetComponent<IHarvestable>();
                if (harvestable != null && !harvestable.IsDestroyed)
                {
                    // Если у нас есть инструмент
                    if (currentTool != null)
                    {
                        harvestable.Harvest(currentTool, gameObject);
                        hitSomething = true;
                    }
                    else if (weapon != null)
                    {
                        // Если используем оружие как инструмент (например, топор-оружие)
                        // Создаем временный ToolData из WeaponData
                        var tempTool = CreateToolFromWeapon(weapon);
                        if (tempTool != null && harvestable.CanBeHarvestedWith(tempTool))
                        {
                            harvestable.Harvest(tempTool, gameObject);
                            hitSomething = true;
                        }
                    }
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

        private ToolData CreateToolFromWeapon(WeaponData weapon)
        {
            // Проверяем, является ли оружие также инструментом (например, боевой топор)
            if (weapon.itemName.ToLower().Contains("axe"))
            {
                var tempTool = ScriptableObject.CreateInstance<ToolData>();
                tempTool.toolType = ToolType.Axe;
                tempTool.damagePerUse = Mathf.RoundToInt(weapon.damage);
                tempTool.efficiency = 0.8f; // Оружие менее эффективно как инструмент
                tempTool.gatherableResources = new ResourceType[] { ResourceType.Wood };
                return tempTool;
            }
            // Можно добавить другие типы оружия-инструментов

            return null;
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