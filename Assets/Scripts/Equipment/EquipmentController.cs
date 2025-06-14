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
        [SerializeField] private SpriteRenderer flipTrackingRenderer; // Явно указываем какой рендерер отслеживать

        [Header("Weapon Display Settings")]
        [SerializeField] private WeaponDisplayConfiguration displayConfig;

        private ItemData currentItemData;
        private GameObject currentWeaponInstance;
        private Animator weaponAnimator;
        private SpriteRenderer weaponRenderer;
        private SpriteRenderer armsRenderer;
        private SpriteRenderer bodyRenderer;

        // Хранение оригинальных контроллеров
        private RuntimeAnimatorController originalBodyController;
        private RuntimeAnimatorController originalArmsController;

        // Текущее состояние
        private int currentDirection = 0;
        private bool isFacingRight = true;
        private Coroutine hideWeaponCoroutine;

        private void Awake()
        {
            Debug.Log("[EquipmentController] Awake called!");
            InitializeReferences();
            CacheOriginalControllers();

            // Создаем дефолтную конфигурацию если не назначена
            if (displayConfig == null)
            {
                Debug.LogWarning("[EquipmentController] No display config assigned, creating default!");
                displayConfig = ScriptableObject.CreateInstance<WeaponDisplayConfiguration>();
                displayConfig.SetDefaultValues();
            }
            else
            {
                Debug.Log("[EquipmentController] Display config loaded successfully!");
            }
        }

        private void InitializeReferences()
        {
            // Если не назначены, пытаемся найти
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

            // Если не указан flipTrackingRenderer, используем bodyRenderer
            if (flipTrackingRenderer == null && bodyRenderer != null)
            {
                flipTrackingRenderer = bodyRenderer;
                Debug.Log("[EquipmentController] Using body renderer for flip tracking");
            }

            // Создаем слот для оружия если не назначен
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

            // Создаем экземпляр оружия
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

            // Применяем анимации к Body
            if (bodyAnimator != null && animSet.bodyAttackFront != null)
            {
                var overrideController = new AnimatorOverrideController(originalBodyController);
                overrideController["Attack_Front"] = animSet.bodyAttackFront;
                overrideController["Attack_Back"] = animSet.bodyAttackBack;
                overrideController["Attack_Side"] = animSet.bodyAttackSide;
                bodyAnimator.runtimeAnimatorController = overrideController;
            }

            // Применяем анимации к Arms
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
            // Возвращаем оригинальные анимации
            if (bodyAnimator != null)
                bodyAnimator.runtimeAnimatorController = originalBodyController;
            if (armsAnimator != null)
                armsAnimator.runtimeAnimatorController = originalArmsController;

            // Удаляем экземпляр оружия
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
            if (currentItemData == null) return;

            // Используем явно указанный рендерер для отслеживания флипа
            SpriteRenderer trackingRenderer = flipTrackingRenderer != null ? flipTrackingRenderer : bodyRenderer;

            // Зафиксировать направление атаки
            currentDirection = bodyAnimator.GetInteger("Direction");
            isFacingRight = trackingRenderer != null ? !trackingRenderer.flipX : true;

            ShowWeapon(); // обновит отображение и позицию оружия

            // Установить направление в weaponAnimator ДО вызова триггера
            if (weaponAnimator != null)
                weaponAnimator.SetInteger("Direction", currentDirection);

            // Атакующие триггеры
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

            Debug.Log($"Attacking with: {currentItemData.itemName}, direction = {currentDirection}");
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

            // Используем явно указанный рендерер для отслеживания флипа
            SpriteRenderer trackingRenderer = flipTrackingRenderer != null ? flipTrackingRenderer : bodyRenderer;
            if (trackingRenderer == null)
            {
                Debug.LogError("[EquipmentController] No renderer to track flip!");
                return;
            }

            // Получаем направление и направление взгляда
            currentDirection = bodyAnimator.GetInteger("Direction");
            bool rendererFlipX = trackingRenderer.flipX;
            isFacingRight = !rendererFlipX;

            // Базовые значения
            Vector2 positionOffset = Vector2.zero;
            float rotationZ = 0f;
            int sortingOffset = 0;
            bool flipX = false;
            bool flipY = false;

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
                    // Флип оружия когда trackingRenderer.flipX = false (смотрит вправо)
                    flipX = !rendererFlipX;

                    Debug.Log($"[SIDE ATTACK] Tracking {trackingRenderer.name}.flipX = {rendererFlipX}, Setting Weapon.flipX = {flipX}");
                    break;

                default:
                    break;
            }

            // Применяем позицию и поворот
            currentWeaponInstance.transform.localPosition = positionOffset;
            currentWeaponInstance.transform.localRotation = Quaternion.Euler(0, 0, rotationZ);

            // Применяем flip
            weaponRenderer.flipX = flipX;
            weaponRenderer.flipY = flipY;

            // Применяем сортировку
            weaponRenderer.sortingLayerName = armsRenderer.sortingLayerName;
            weaponRenderer.sortingOrder = armsRenderer.sortingOrder + sortingOffset;

            Debug.Log($"[WeaponDisplay] Dir: {currentDirection}, Tracking: {trackingRenderer.name}, TrackerFlip: {rendererFlipX}, WeaponFlip: {flipX}");
        }

        private WeaponPositionSettings GetCurrentPositionSettings()
        {
            switch (currentDirection)
            {
                case 0: // Front
                    return displayConfig.frontAttackSettings;
                case 1: // Back
                    return displayConfig.backAttackSettings;
                case 2: // Side
                    return isFacingRight ? displayConfig.sideRightSettings : displayConfig.sideLeftSettings;
                default:
                    return displayConfig.sideRightSettings;
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
            if (animSet != null)
                return animSet.attackDuration;

            return 1f; // Дефолтная длительность
        }

        public ItemData GetCurrentItem() => currentItemData;
        public bool HasItemEquipped() => currentItemData != null;

        private void Update()
        {
            // Обновляем отображение оружия если оно активно
            if (currentWeaponInstance != null && currentWeaponInstance.activeSelf)
            {
                // Используем явно указанный рендерер для отслеживания флипа
                SpriteRenderer trackingRenderer = flipTrackingRenderer != null ? flipTrackingRenderer : bodyRenderer;

                // Проверяем изменение направления
                int newDirection = bodyAnimator.GetInteger("Direction");
                bool newFacingRight = trackingRenderer != null ? !trackingRenderer.flipX : true;

                if (newDirection != currentDirection || newFacingRight != isFacingRight)
                {
                    currentDirection = newDirection;
                    isFacingRight = newFacingRight;
                    UpdateWeaponDisplay();
                }

                // Синхронизируем параметры аниматора
                if (weaponAnimator != null)
                {
                    weaponAnimator.SetInteger("Direction", currentDirection);

                    // ВАЖНО: Проверяем не перезаписывает ли аниматор флип
                    if (currentDirection == 2 && weaponRenderer != null && trackingRenderer != null)
                    {
                        bool expectedFlip = !trackingRenderer.flipX;
                        if (weaponRenderer.flipX != expectedFlip)
                        {
                            Debug.LogWarning($"[FLIP OVERRIDE DETECTED] Animator is overriding flip! Forcing flip to {expectedFlip}");
                            weaponRenderer.flipX = expectedFlip;
                        }
                    }
                }
            }
        }
    }
}