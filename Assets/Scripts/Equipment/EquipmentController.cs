using UnityEngine;
using Combat.Data; // Добавляем namespace для WeaponAnimationSet

namespace Player.Equipment
{
    public class EquipmentController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator bodyAnimator;
        [SerializeField] private Animator armsAnimator;
        [SerializeField] private Transform weaponSlot; // Слот для спавна оружия во время атаки

        private ItemData currentItemData;
        private GameObject currentWeaponInstance; // Экземпляр оружия для анимации
        private Animator weaponAnimator; // Аниматор оружия

        // Хранение оригинальных контроллеров
        private RuntimeAnimatorController originalBodyController;
        private RuntimeAnimatorController originalArmsController;

        private void Awake()
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
                            bodyAnimator = bodyTransform.GetComponent<Animator>();
                    }

                    if (armsAnimator == null)
                    {
                        Transform armsTransform = parent.Find("Arms");
                        if (armsTransform != null)
                            armsAnimator = armsTransform.GetComponent<Animator>();
                    }
                }
            }

            // Создаем слот для оружия если не назначен
            if (weaponSlot == null)
            {
                GameObject slot = new GameObject("WeaponSlot");
                slot.transform.SetParent(transform);
                slot.transform.localPosition = Vector3.zero;
                weaponSlot = slot.transform;

                Debug.Log($"Created WeaponSlot at {weaponSlot.position}");
            }

            // Сохраняем оригинальные контроллеры
            if (bodyAnimator != null)
                originalBodyController = bodyAnimator.runtimeAnimatorController;
            if (armsAnimator != null)
                originalArmsController = armsAnimator.runtimeAnimatorController;
        }

        /// <summary>
        /// Экипирует предмет
        /// </summary>
        public void EquipItem(ItemData itemData)
        {
            if (itemData == null) return;

            // Снимаем текущий предмет
            UnequipCurrentItem();

            currentItemData = itemData;

            // Создаем экземпляр оружия (но пока скрытый)
            GameObject prefabToSpawn = null;

            if (itemData is WeaponData weaponData && weaponData.weaponPrefab != null)
            {
                prefabToSpawn = weaponData.weaponPrefab;
            }
            else if (itemData is ToolData toolData && toolData.toolPrefab != null)
            {
                prefabToSpawn = toolData.toolPrefab;
            }

            if (prefabToSpawn != null)
            {
                currentWeaponInstance = Instantiate(prefabToSpawn, weaponSlot);
                currentWeaponInstance.SetActive(false); // Скрываем до атаки
                weaponAnimator = currentWeaponInstance.GetComponent<Animator>();
            }

            // Применяем анимации персонажа
            ApplyCharacterAnimations();

            Debug.Log($"Equipped: {itemData.itemName}");
        }

        /// <summary>
        /// Применяет анимации Body и Arms для текущего предмета
        /// </summary>
        private void ApplyCharacterAnimations()
        {
            WeaponAnimationSet animSet = null;

            // Получаем набор анимаций
            if (currentItemData is WeaponData weaponData)
            {
                animSet = weaponData.animationSet;
            }
            else if (currentItemData is ToolData toolData)
            {
                animSet = toolData.animationSet;
            }

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

        /// <summary>
        /// Снимает текущий экипированный предмет
        /// </summary>
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
                Destroy(currentWeaponInstance);
                currentWeaponInstance = null;
                weaponAnimator = null;
            }

            currentItemData = null;

            Debug.Log("Unequipped current item");
        }

        /// <summary>
        /// Запускает атаку с текущим предметом
        /// </summary>
        public void Attack()
        {
            if (currentItemData == null)
            {
                Debug.Log("No item equipped for attack");
                return;
            }

            // Запускаем анимации на всех трех слоях
            if (bodyAnimator != null)
                bodyAnimator.SetTrigger("Attack");
            if (armsAnimator != null)
                armsAnimator.SetTrigger("Attack");
            if (weaponAnimator != null && currentWeaponInstance != null)
            {
                // Показываем оружие
                currentWeaponInstance.SetActive(true);

                // Настраиваем сортировку для видимости
                var weaponRenderer = currentWeaponInstance.GetComponent<SpriteRenderer>();
                var armsRenderer = armsAnimator.GetComponent<SpriteRenderer>();

                if (weaponRenderer != null && armsRenderer != null)
                {
                    // Оружие должно быть поверх рук
                    weaponRenderer.sortingLayerName = armsRenderer.sortingLayerName;
                    weaponRenderer.sortingOrder = armsRenderer.sortingOrder + 1;

                    // Синхронизируем флип с телом для Side анимаций
                    int direction = bodyAnimator.GetInteger("Direction");
                    if (direction == 2) // Side
                    {
                        var bodyRenderer = bodyAnimator.GetComponent<SpriteRenderer>();
                        if (bodyRenderer != null)
                        {
                            weaponRenderer.flipX = bodyRenderer.flipX;
                        }
                    }
                    else
                    {
                        weaponRenderer.flipX = false;
                    }

                    Debug.Log($"Weapon sorting: Layer={weaponRenderer.sortingLayerName}, Order={weaponRenderer.sortingOrder}");
                }

                // Позиционируем оружие правильно
                currentWeaponInstance.transform.localPosition = Vector3.zero;
                currentWeaponInstance.transform.localRotation = Quaternion.identity;
                currentWeaponInstance.transform.localScale = Vector3.one;

                // Запускаем анимацию оружия
                weaponAnimator.SetTrigger("Attack");

                Debug.Log($"Weapon active: {currentWeaponInstance.activeSelf}, Position: {currentWeaponInstance.transform.position}");

                // Планируем скрытие оружия после анимации
                float animationDuration = GetAttackAnimationDuration();
                CancelInvoke(nameof(HideWeapon));
                Invoke(nameof(HideWeapon), animationDuration);
            }
            else
            {
                Debug.LogWarning("No weapon animator or instance!");
            }

            Debug.Log($"Attacking with: {currentItemData.itemName}");
        }

        /// <summary>
        /// Скрывает оружие после атаки
        /// </summary>
        private void HideWeapon()
        {
            if (currentWeaponInstance != null)
                currentWeaponInstance.SetActive(false);
        }

        /// <summary>
        /// Получает длительность анимации атаки
        /// </summary>
        private float GetAttackAnimationDuration()
        {
            // Можно настроить для каждого оружия отдельно
            if (currentItemData is WeaponData weaponData && weaponData.animationSet != null)
            {
                return weaponData.animationSet.attackDuration;
            }
            else if (currentItemData is ToolData toolData && toolData.animationSet != null)
            {
                return toolData.animationSet.attackDuration;
            }

            return 1f; // Дефолтная длительность
        }

        /// <summary>
        /// Возвращает данные текущего экипированного предмета
        /// </summary>
        public ItemData GetCurrentItem() => currentItemData;

        /// <summary>
        /// Проверяет, экипирован ли предмет
        /// </summary>
        public bool HasItemEquipped() => currentItemData != null;

        private void Update()
        {
            // Синхронизация направления и флипа для оружия
            if (weaponAnimator != null && currentWeaponInstance != null && currentWeaponInstance.activeSelf)
            {
                // Берем направление из body аниматора
                int direction = bodyAnimator.GetInteger("Direction");
                weaponAnimator.SetInteger("Direction", direction);

                // Синхронизируем флип для Side направления
                if (direction == 2) // Side
                {
                    var weaponRenderer = currentWeaponInstance.GetComponent<SpriteRenderer>();
                    var bodyRenderer = bodyAnimator.GetComponent<SpriteRenderer>();

                    if (weaponRenderer != null && bodyRenderer != null)
                    {
                        weaponRenderer.flipX = bodyRenderer.flipX;
                    }
                }
            }
        }
    }
}