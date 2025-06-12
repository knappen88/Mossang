namespace Combat.Equipment
{
    using UnityEngine;
    using UnityEngine.Events;
    using Combat.Core;
    using Combat.Data;
    using System.Collections.Generic;
    using System.Linq;

    public class WeaponEquipmentController : MonoBehaviour
    {
        [Header("Weapon Slots")]
        [SerializeField] private Combat.Core.WeaponSlot primaryWeaponSlot;

        [Header("References")]
        [SerializeField] private Animator bodyAnimator;
        [SerializeField] private Animator armsAnimator;
        [SerializeField] private PlayerAnimator playerAnimator; // Для получения текущего направления

        [Header("Events")]
        public UnityEvent<WeaponData> OnWeaponEquipped;
        public UnityEvent<WeaponData> OnWeaponUnequipped;

        private GameObject currentWeaponInstance;
        private WeaponData currentWeaponData;
        private WeaponAnimationSet currentAnimationSet;
        private Animator weaponAnimator;

        private AnimatorOverrideController bodyOverrideController;
        private AnimatorOverrideController armsOverrideController;

        private void Awake()
        {
            InitializeAnimatorControllers();
        }

        private void InitializeAnimatorControllers()
        {
            // Создаем override контроллеры
            if (bodyAnimator && bodyAnimator.runtimeAnimatorController != null)
            {
                bodyOverrideController = new AnimatorOverrideController(bodyAnimator.runtimeAnimatorController);
                bodyAnimator.runtimeAnimatorController = bodyOverrideController;
            }

            if (armsAnimator && armsAnimator.runtimeAnimatorController != null)
            {
                armsOverrideController = new AnimatorOverrideController(armsAnimator.runtimeAnimatorController);
                armsAnimator.runtimeAnimatorController = armsOverrideController;
            }
        }

        public void EquipWeapon(WeaponData weaponData)
        {
            if (weaponData == null) return;

            UnequipCurrentWeapon();

            currentWeaponData = weaponData;

            // Создаем экземпляр оружия
            if (weaponData.weaponPrefab != null && primaryWeaponSlot.slotTransform != null)
            {
                currentWeaponInstance = Instantiate(
                    weaponData.weaponPrefab,
                    primaryWeaponSlot.slotTransform
                );

                currentWeaponInstance.transform.localPosition = primaryWeaponSlot.localPosition;
                currentWeaponInstance.transform.localRotation = Quaternion.Euler(primaryWeaponSlot.localRotation);
                currentWeaponInstance.transform.localScale = primaryWeaponSlot.localScale;

                weaponAnimator = currentWeaponInstance.GetComponent<Animator>();

                var equippable = currentWeaponInstance.GetComponent<IEquippable>();
                equippable?.OnEquip(gameObject);
            }

            // Применяем анимации атаки
            if (weaponData.animationSet != null)
            {
                currentAnimationSet = weaponData.animationSet;
                ApplyWeaponAttackAnimations();
            }

            OnWeaponEquipped?.Invoke(weaponData);
        }

        public void UnequipCurrentWeapon()
        {
            if (currentWeaponInstance != null)
            {
                var equippable = currentWeaponInstance.GetComponent<IEquippable>();
                equippable?.OnUnequip();

                Destroy(currentWeaponInstance);
                currentWeaponInstance = null;
            }

            if (currentWeaponData != null)
            {
                OnWeaponUnequipped?.Invoke(currentWeaponData);
                currentWeaponData = null;
            }

            RestoreDefaultAnimations();
        }

        private void ApplyWeaponAttackAnimations()
        {
            if (currentAnimationSet == null) return;

            // Применяем анимации атаки для всех направлений
            ApplyDirectionalAttackAnimations();
        }

        private void ApplyDirectionalAttackAnimations()
        {
            // Для тела
            if (bodyOverrideController != null)
            {
                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

                // Attack_Front
                var attackFrontClip = GetClipByName(bodyOverrideController, "Attack_Front");
                if (attackFrontClip != null && currentAnimationSet.bodyAttackFront != null)
                {
                    overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(
                        attackFrontClip, currentAnimationSet.bodyAttackFront));
                }

                // Attack_Side
                var attackSideClip = GetClipByName(bodyOverrideController, "Attack_Side");
                if (attackSideClip != null && currentAnimationSet.bodyAttackSide != null)
                {
                    overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(
                        attackSideClip, currentAnimationSet.bodyAttackSide));
                }

                // Attack_Back
                var attackBackClip = GetClipByName(bodyOverrideController, "Attack_Back");
                if (attackBackClip != null && currentAnimationSet.bodyAttackBack != null)
                {
                    overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(
                        attackBackClip, currentAnimationSet.bodyAttackBack));
                }

                if (overrides.Count > 0)
                    bodyOverrideController.ApplyOverrides(overrides);
            }

            // Для рук
            if (armsOverrideController != null)
            {
                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

                // Attack_Front
                var attackFrontClip = GetClipByName(armsOverrideController, "Attack_Front");
                if (attackFrontClip != null && currentAnimationSet.armsAttackFront != null)
                {
                    overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(
                        attackFrontClip, currentAnimationSet.armsAttackFront));
                }

                // Attack_Side
                var attackSideClip = GetClipByName(armsOverrideController, "Attack_Side");
                if (attackSideClip != null && currentAnimationSet.armsAttackSide != null)
                {
                    overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(
                        attackSideClip, currentAnimationSet.armsAttackSide));
                }

                // Attack_Back
                var attackBackClip = GetClipByName(armsOverrideController, "Attack_Back");
                if (attackBackClip != null && currentAnimationSet.armsAttackBack != null)
                {
                    overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(
                        attackBackClip, currentAnimationSet.armsAttackBack));
                }

                if (overrides.Count > 0)
                    armsOverrideController.ApplyOverrides(overrides);
            }
        }

        private void RestoreDefaultAnimations()
        {
            if (bodyOverrideController != null)
            {
                bodyOverrideController.ApplyOverrides(new List<KeyValuePair<AnimationClip, AnimationClip>>());
            }

            if (armsOverrideController != null)
            {
                armsOverrideController.ApplyOverrides(new List<KeyValuePair<AnimationClip, AnimationClip>>());
            }

            currentAnimationSet = null;
        }

        private AnimationClip GetClipByName(AnimatorOverrideController controller, string clipName)
        {
            return controller.animationClips.FirstOrDefault(clip => clip.name.Contains(clipName));
        }

        public void TriggerAttack()
        {
            if (currentWeaponData == null) return;

            // Запускаем анимации атаки с учетом направления
            bodyAnimator?.SetTrigger("Attack");
            armsAnimator?.SetTrigger("Attack");
            weaponAnimator?.SetTrigger("Attack");

            if (currentAnimationSet != null)
            {
                StartCoroutine(AttackCoroutine());
            }
        }

        private System.Collections.IEnumerator AttackCoroutine()
        {
            yield return new WaitForSeconds(currentAnimationSet.GetAttackHitTime());
            // Событие удара обрабатывается в WeaponCombatHandler
        }

        public WeaponData GetCurrentWeapon() => currentWeaponData;
        public WeaponAnimationSet GetCurrentAnimationSet() => currentAnimationSet;
        public bool HasWeaponEquipped() => currentWeaponData != null;
    }
}