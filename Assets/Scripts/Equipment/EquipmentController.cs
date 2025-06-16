using UnityEngine;
using Combat.Data;
using System.Collections;

namespace Player.Equipment
{
    public class EquipmentController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator bodyAnimator;
        [SerializeField] private Animator armsAnimator;
        [SerializeField] private Transform weaponSlot;

        [Header("Flip Tracking")]
        [SerializeField] private SpriteRenderer flipTrackingRenderer;

        [Header("Weapon Display Settings")]
        [SerializeField] private WeaponDisplayConfiguration displayConfig;

        [Header("Attack Settings")]
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private bool useWeaponAttackSpeed = true;

        private ItemData currentItemData;
        private GameObject currentWeaponInstance;
        private Animator weaponAnimator;
        private SpriteRenderer weaponRenderer;
        private SpriteRenderer armsRenderer;
        private SpriteRenderer bodyRenderer;

        private RuntimeAnimatorController originalBodyController;
        private RuntimeAnimatorController originalArmsController;

        private int currentDirection = 0;
        private bool isFacingRight = true;
        private Coroutine hideWeaponCoroutine;

        private float lastAttackTime = 0f;
        private bool isAttacking = false;

        private void Awake()
        {
            Debug.Log("[EquipmentController] Awake called!");
            InitializeReferences();
            CacheOriginalControllers();

            if (displayConfig == null)
            {
                Debug.LogWarning("[EquipmentController] No display config assigned, creating default!");
                displayConfig = ScriptableObject.CreateInstance<WeaponDisplayConfiguration>();
                displayConfig.SetDefaultValues();
            }
        }

        private void InitializeReferences()
        {
            if (bodyAnimator == null || armsAnimator == null)
            {
                Transform parent = transform.parent;
                if (parent != null)
                {
                    if (bodyAnimator == null)
                    {
                        Transform bodyTransform = parent.Find("Body");
                        if (bodyTransform != null)
                        {
                            bodyAnimator = bodyTransform.GetComponent<Animator>();
                            bodyRenderer = bodyAnimator.GetComponent<SpriteRenderer>();
                        }
                    }

                    if (armsAnimator == null)
                    {
                        Transform armsTransform = parent.Find("Arms");
                        if (armsTransform != null)
                        {
                            armsAnimator = armsTransform.GetComponent<Animator>();
                            armsRenderer = armsAnimator.GetComponent<SpriteRenderer>();
                        }
                    }
                }
            }

            if (flipTrackingRenderer == null && bodyRenderer != null)
            {
                flipTrackingRenderer = bodyRenderer;
                Debug.Log("[EquipmentController] Using body renderer for flip tracking");
            }

            if (weaponSlot == null)
            {
                GameObject slot = new GameObject("WeaponSlot");
                slot.transform.SetParent(transform);
                slot.transform.localPosition = Vector3.zero;
                weaponSlot = slot.transform;
            }
        }

        private void CacheOriginalControllers()
        {
            if (bodyAnimator != null)
                originalBodyController = bodyAnimator.runtimeAnimatorController;
            if (armsAnimator != null)
                originalArmsController = armsAnimator.runtimeAnimatorController;
        }

        public void EquipItem(ItemData itemData)
        {
            if (itemData == null) return;

            UnequipCurrentItem();
            currentItemData = itemData;

            GameObject prefabToSpawn = GetWeaponPrefab(itemData);

            if (prefabToSpawn != null)
            {
                currentWeaponInstance = Instantiate(prefabToSpawn, weaponSlot);
                currentWeaponInstance.SetActive(false);
                weaponAnimator = currentWeaponInstance.GetComponent<Animator>();
                weaponRenderer = currentWeaponInstance.GetComponent<SpriteRenderer>();
            }

            ApplyCharacterAnimations();
            Debug.Log($"Equipped: {itemData.itemName}");
        }

        private GameObject GetWeaponPrefab(ItemData itemData)
        {
            if (itemData is WeaponData weaponData && weaponData.weaponPrefab != null)
                return weaponData.weaponPrefab;
            else if (itemData is ToolData toolData && toolData.toolPrefab != null)
                return toolData.toolPrefab;

            return null;
        }

        private void ApplyCharacterAnimations()
        {
            WeaponAnimationSet animSet = GetAnimationSet();
            if (animSet == null) return;

            if (bodyAnimator != null && animSet.bodyAttackFront != null)
            {
                var overrideController = new AnimatorOverrideController(originalBodyController);
                overrideController["Attack_Front"] = animSet.bodyAttackFront;
                overrideController["Attack_Back"] = animSet.bodyAttackBack;
                overrideController["Attack_Side"] = animSet.bodyAttackSide;
                bodyAnimator.runtimeAnimatorController = overrideController;
            }

            if (armsAnimator != null && animSet.armsAttackFront != null)
            {
                var overrideController = new AnimatorOverrideController(originalArmsController);
                overrideController["Attack_Front"] = animSet.armsAttackFront;
                overrideController["Attack_Back"] = animSet.armsAttackBack;
                overrideController["Attack_Side"] = animSet.armsAttackSide;
                armsAnimator.runtimeAnimatorController = overrideController;
            }
        }

        private WeaponAnimationSet GetAnimationSet()
        {
            if (currentItemData is WeaponData weaponData)
                return weaponData.animationSet;
            else if (currentItemData is ToolData toolData)
                return toolData.animationSet;

            return null;
        }

        public void UnequipCurrentItem()
        {
            if (bodyAnimator != null)
                bodyAnimator.runtimeAnimatorController = originalBodyController;
            if (armsAnimator != null)
                armsAnimator.runtimeAnimatorController = originalArmsController;

            if (currentWeaponInstance != null)
            {
                if (hideWeaponCoroutine != null)
                {
                    StopCoroutine(hideWeaponCoroutine);
                    hideWeaponCoroutine = null;
                }

                Destroy(currentWeaponInstance);
                currentWeaponInstance = null;
                weaponAnimator = null;
                weaponRenderer = null;
            }

            currentItemData = null;
        }

        public void Attack()
        {
            if (!CanAttack()) return;
            if (currentItemData == null) return;

            SpriteRenderer trackingRenderer = flipTrackingRenderer != null ? flipTrackingRenderer : bodyRenderer;

            if (trackingRenderer == null)
            {
                Debug.LogError("[EquipmentController] No renderer available for flip tracking!");
                return;
            }

            if (bodyAnimator == null || armsAnimator == null)
            {
                Debug.LogError("[EquipmentController] Missing animators! Cannot perform attack.");
                return;
            }

            isAttacking = true;
            lastAttackTime = Time.time;

            currentDirection = bodyAnimator.GetInteger("Direction");
            isFacingRight = !trackingRenderer.flipX;

            ShowWeapon();

            if (weaponAnimator != null)
                weaponAnimator.SetInteger("Direction", currentDirection);

            bodyAnimator.SetTrigger("Attack");
            armsAnimator.SetTrigger("Attack");

            if (weaponAnimator != null)
            {
                weaponAnimator.SetTrigger("Attack");

                float animationDuration = GetAttackAnimationDuration();
                if (hideWeaponCoroutine != null)
                    StopCoroutine(hideWeaponCoroutine);
                hideWeaponCoroutine = StartCoroutine(HideWeaponAfterDelay(animationDuration));
            }

            // Выполняем проверку попаданий СРАЗУ (можно добавить задержку если нужно)
            StartCoroutine(PerformAttackWithDelay(0.2f)); // Небольшая задержка для синхронизации с анимацией

            StartCoroutine(ResetAttackFlag());

            Debug.Log($"Attacking with: {currentItemData.itemName}, direction = {currentDirection}");
        }

        private IEnumerator PerformAttackWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            PerformAttackCheck();
        }

        private void PerformAttackCheck()
        {
            // Определяем точку атаки
            Vector2 attackOffset = Vector2.zero;
            float attackRange = 1.5f;

            switch (currentDirection)
            {
                case 0: // Front
                    attackOffset = new Vector2(0, -0.5f);
                    break;
                case 1: // Back
                    attackOffset = new Vector2(0, 0.5f);
                    break;
                case 2: // Side
                    attackOffset = new Vector2(isFacingRight ? 0.5f : -0.5f, 0);
                    break;
            }

            Vector2 attackPos = (Vector2)transform.position + attackOffset;

            // Проверяем тип экипированного предмета
            if (currentItemData is ToolData tool)
            {
                // Для инструментов
                attackRange = tool.gatherRadius;

                // Ищем все объекты в радиусе
                Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange);

                Debug.Log($"[Tool Attack] Found {hits.Length} objects in range {attackRange}");
                Debug.Log($"[Tool Attack] Using tool: {tool.itemName}, Type: {tool.toolType}");
                Debug.Log($"[Tool Attack] Can gather: {string.Join(", ", tool.gatherableResources)}");

                foreach (var hit in hits)
                {
                    if (hit.gameObject == gameObject) continue;

                    Debug.Log($"[Tool Attack] Checking object: {hit.name}");

                    // ВАЖНО: Проверяем универсальный интерфейс IHarvestable
                    var harvestable = hit.GetComponent<IHarvestable>();
                    if (harvestable != null && !harvestable.IsDestroyed)
                    {
                        Debug.Log($"[Tool Attack] Found IHarvestable on {hit.name}");
                        Debug.Log($"[Tool Attack] Resource type: {harvestable.GetResourceType()}");
                        Debug.Log($"[Tool Attack] Can harvest with {tool.toolType}: {harvestable.CanBeHarvestedWith(tool)}");

                        // Вызываем метод сбора
                        harvestable.Harvest(tool, gameObject);

                        // Воспроизводим звук удара если был успешный сбор
                        if (harvestable.CanBeHarvestedWith(tool))
                        {
                            PlayToolHitSound(tool);
                            Debug.Log($"[Tool Attack] Successfully harvested {hit.name}!");
                        }
                    }
                    else
                    {
                        Debug.Log($"[Tool Attack] No IHarvestable interface on {hit.name} or already destroyed");
                    }
                }
            }
            else if (currentItemData is WeaponData weapon)
            {
                // Для оружия
                attackRange = weapon.attackRange;

                Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange);

                foreach (var hit in hits)
                {
                    if (hit.gameObject == gameObject) continue;

                    var damageable = hit.GetComponent<IDamageable>();
                    if (damageable != null && damageable.IsAlive)
                    {
                        damageable.TakeDamage(weapon.damage, transform.position);
                    }
                }
            }

            // Отладочная визуализация (будет видна в Scene view)
            Debug.DrawLine(attackPos + Vector2.left * attackRange, attackPos + Vector2.right * attackRange, Color.red, 1f);
            Debug.DrawLine(attackPos + Vector2.up * attackRange, attackPos + Vector2.down * attackRange, Color.red, 1f);
        }

        private bool CanAttack()
        {
            if (isAttacking)
            {
                Debug.Log("[EquipmentController] Already attacking!");
                return false;
            }

            float cooldown = GetAttackCooldown();

            if (Time.time - lastAttackTime < cooldown)
            {
                float remainingTime = cooldown - (Time.time - lastAttackTime);
                Debug.Log($"[EquipmentController] Attack on cooldown! Wait {remainingTime:F2}s");
                return false;
            }

            return true;
        }

        private float GetAttackCooldown()
        {
            if (!useWeaponAttackSpeed || currentItemData == null)
            {
                return attackCooldown;
            }

            if (currentItemData is WeaponData weapon)
            {
                return weapon.attackSpeed > 0 ? 1f / weapon.attackSpeed : attackCooldown;
            }

            if (currentItemData is ToolData tool)
            {
                return 1f; // Инструменты медленнее
            }

            return attackCooldown;
        }

        private IEnumerator ResetAttackFlag()
        {
            float animationDuration = GetAttackAnimationDuration();
            yield return new WaitForSeconds(animationDuration);
            isAttacking = false;
        }

        private void PlayToolHitSound(ToolData tool)
        {
            if (tool.hitSounds != null && tool.hitSounds.Length > 0)
            {
                var audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }

                var randomSound = tool.hitSounds[Random.Range(0, tool.hitSounds.Length)];
                audioSource.PlayOneShot(randomSound);
            }
        }

        private void ShowWeapon()
        {
            if (currentWeaponInstance == null) return;
            currentWeaponInstance.SetActive(true);
            UpdateWeaponDisplay();
        }

        private void UpdateWeaponDisplay()
        {
            if (weaponRenderer == null || armsRenderer == null) return;

            SpriteRenderer trackingRenderer = flipTrackingRenderer != null ? flipTrackingRenderer : bodyRenderer;
            if (trackingRenderer == null) return;

            currentDirection = bodyAnimator.GetInteger("Direction");
            bool rendererFlipX = trackingRenderer.flipX;
            isFacingRight = !rendererFlipX;

            Vector2 positionOffset = Vector2.zero;
            float rotationZ = 0f;
            int sortingOffset = 0;
            bool flipX = false;
            bool flipY = false;

            // Проверяем, является ли текущий предмет киркой
            bool isPickaxe = false;
            if (currentItemData is ToolData tool && tool.toolType == ToolType.Pickaxe)
            {
                isPickaxe = true;
            }

            switch (currentDirection)
            {
                case 0: // Front
                    positionOffset = new Vector2(0f, 0.3f);
                    rotationZ = -90f;
                    sortingOffset = 2;
                    break;

                case 1: // Back
                    positionOffset = new Vector2(0f, -0.3f);
                    rotationZ = 90f;
                    sortingOffset = -2;
                    break;

                case 2: // Side
                    positionOffset = isFacingRight ? new Vector2(0.5f, 0f) : new Vector2(-0.5f, 0f);
                    rotationZ = 0f;
                    sortingOffset = isFacingRight ? 1 : -1;

                    // Специальная логика flip для кирки
                    if (isPickaxe)
                    {
                        // Для кирки делаем обратный flip
                        flipX = rendererFlipX; // Прямая зависимость, а не инвертированная
                    }
                    else
                    {
                        // Для остальных инструментов оставляем как было
                        flipX = !rendererFlipX;
                    }
                    break;
            }

            currentWeaponInstance.transform.localPosition = positionOffset;
            currentWeaponInstance.transform.localRotation = Quaternion.Euler(0, 0, rotationZ);

            weaponRenderer.flipX = flipX;
            weaponRenderer.flipY = flipY;

            weaponRenderer.sortingLayerName = armsRenderer.sortingLayerName;
            weaponRenderer.sortingOrder = armsRenderer.sortingOrder + sortingOffset;

            // Отладочный вывод для проверки
            if (isPickaxe && currentDirection == 2)
            {
                Debug.Log($"[Pickaxe Flip] Facing: {(isFacingRight ? "RIGHT" : "LEFT")}, Renderer FlipX: {rendererFlipX}, Weapon FlipX: {flipX}");
            }
        }

        private IEnumerator HideWeaponAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideWeapon();
        }

        private void HideWeapon()
        {
            if (currentWeaponInstance != null)
                currentWeaponInstance.SetActive(false);
        }

        private float GetAttackAnimationDuration()
        {
            var animSet = GetAnimationSet();
            return animSet != null ? animSet.attackDuration : 1f;
        }

        public ItemData GetCurrentItem() => currentItemData;
        public bool HasItemEquipped() => currentItemData != null;
        public float GetAttackCooldownProgress()
        {
            float cooldown = GetAttackCooldown();
            float timeSinceLastAttack = Time.time - lastAttackTime;
            return Mathf.Clamp01(timeSinceLastAttack / cooldown);
        }
        public bool IsAttackReady() => CanAttack();

        private void Update()
        {
            if (currentWeaponInstance != null && currentWeaponInstance.activeSelf)
            {
                SpriteRenderer trackingRenderer = flipTrackingRenderer != null ? flipTrackingRenderer : bodyRenderer;

                int newDirection = bodyAnimator.GetInteger("Direction");
                bool newFacingRight = trackingRenderer != null ? !trackingRenderer.flipX : true;

                if (newDirection != currentDirection || newFacingRight != isFacingRight)
                {
                    currentDirection = newDirection;
                    isFacingRight = newFacingRight;
                    UpdateWeaponDisplay();
                }

                if (weaponAnimator != null)
                {
                    weaponAnimator.SetInteger("Direction", currentDirection);

                    // ВАЖНО: Проверяем тип инструмента для правильной логики flip
                    if (currentDirection == 2 && weaponRenderer != null && trackingRenderer != null)
                    {
                        bool isPickaxe = false;
                        if (currentItemData is ToolData tool && tool.toolType == ToolType.Pickaxe)
                        {
                            isPickaxe = true;
                        }

                        bool expectedFlip;
                        if (isPickaxe)
                        {
                            // Для кирки используем прямую логику
                            expectedFlip = trackingRenderer.flipX;
                        }
                        else
                        {
                            // Для остальных инструментов инвертированная логика
                            expectedFlip = !trackingRenderer.flipX;
                        }

                        if (weaponRenderer.flipX != expectedFlip)
                        {
                            Debug.LogWarning($"[FLIP OVERRIDE DETECTED] Tool: {currentItemData?.itemName}, Animator is overriding flip! Forcing flip to {expectedFlip}");
                            weaponRenderer.flipX = expectedFlip;
                        }
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            Vector2 attackOffset = Vector2.zero;
            float attackRange = 1.5f;

            switch (currentDirection)
            {
                case 0: attackOffset = new Vector2(0, -0.5f); break;
                case 1: attackOffset = new Vector2(0, 0.5f); break;
                case 2: attackOffset = new Vector2(isFacingRight ? 0.5f : -0.5f, 0); break;
            }

            Vector2 attackPos = (Vector2)transform.position + attackOffset;

            if (currentItemData is WeaponData weapon)
                attackRange = weapon.attackRange;
            else if (currentItemData is ToolData tool)
                attackRange = tool.gatherRadius;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPos, attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPos, 0.1f);
        }

        private void OnDestroy()
        {
            if (hideWeaponCoroutine != null)
            {
                StopCoroutine(hideWeaponCoroutine);
            }
        }
    }
}