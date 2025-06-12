namespace Combat.Data
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Combat/Weapon Animation Set", fileName = "New Weapon Animation Set")]
    public class WeaponAnimationSet : ScriptableObject
    {
        [Header("Attack Animations - Body")]
        [Tooltip("Анимация атаки вперед для тела")]
        public AnimationClip bodyAttackFront;
        [Tooltip("Анимация атаки в сторону для тела")]
        public AnimationClip bodyAttackSide;
        [Tooltip("Анимация атаки назад для тела")]
        public AnimationClip bodyAttackBack;

        [Header("Attack Animations - Arms")]
        [Tooltip("Анимация атаки вперед для рук")]
        public AnimationClip armsAttackFront;
        [Tooltip("Анимация атаки в сторону для рук")]
        public AnimationClip armsAttackSide;
        [Tooltip("Анимация атаки назад для рук")]
        public AnimationClip armsAttackBack;

        [Header("Attack Animations - Weapon (optional)")]
        [Tooltip("Анимация атаки вперед для оружия")]
        public AnimationClip weaponAttackFront;
        [Tooltip("Анимация атаки в сторону для оружия")]
        public AnimationClip weaponAttackSide;
        [Tooltip("Анимация атаки назад для оружия")]
        public AnimationClip weaponAttackBack;

        [Header("Animation Settings")]
        [Tooltip("Длительность анимации атаки")]
        public float attackDuration = 0.5f;
        [Tooltip("Момент нанесения урона (0-1)")]
        [Range(0f, 1f)]
        public float attackHitTimeNormalized = 0.5f;

        /// <summary>
        /// Получить время в секундах когда происходит удар
        /// </summary>
        public float GetAttackHitTime()
        {
            return attackDuration * attackHitTimeNormalized;
        }

        /// <summary>
        /// Получить анимацию атаки для тела по направлению
        /// </summary>
        public AnimationClip GetBodyAttackAnimation(int direction)
        {
            switch (direction)
            {
                case 0: return bodyAttackFront;  // Front
                case 1: return bodyAttackBack;   // Back
                case 2: return bodyAttackSide;   // Side
                default: return bodyAttackFront;
            }
        }

        /// <summary>
        /// Получить анимацию атаки для рук по направлению
        /// </summary>
        public AnimationClip GetArmsAttackAnimation(int direction)
        {
            switch (direction)
            {
                case 0: return armsAttackFront;  // Front
                case 1: return armsAttackBack;   // Back
                case 2: return armsAttackSide;   // Side
                default: return armsAttackFront;
            }
        }

        /// <summary>
        /// Получить анимацию атаки для оружия по направлению
        /// </summary>
        public AnimationClip GetWeaponAttackAnimation(int direction)
        {
            switch (direction)
            {
                case 0: return weaponAttackFront;  // Front
                case 1: return weaponAttackBack;   // Back
                case 2: return weaponAttackSide;   // Side
                default: return weaponAttackFront;
            }
        }
    }
}
