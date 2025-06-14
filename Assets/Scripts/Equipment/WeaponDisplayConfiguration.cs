using UnityEngine;

namespace Player.Equipment
{
    // ScriptableObject для настроек отображения оружия
    [CreateAssetMenu(fileName = "WeaponDisplayConfig", menuName = "Combat/WeaponDisplayConfiguration")]
    public class WeaponDisplayConfiguration : ScriptableObject
    {
        [Header("Attack Direction Settings")]
        public WeaponPositionSettings sideRightSettings = new WeaponPositionSettings
        {
            positionOffset = new Vector2(0.5f, 0),
            rotationZ = 0,
            sortingOrderOffset = 1,
            flipX = false,
            flipY = false
        };

        public WeaponPositionSettings sideLeftSettings = new WeaponPositionSettings
        {
            positionOffset = new Vector2(-0.5f, 0),
            rotationZ = 0,
            sortingOrderOffset = -1,
            flipX = true,
            flipY = false
        };

        public WeaponPositionSettings frontAttackSettings = new WeaponPositionSettings
        {
            positionOffset = new Vector2(0, 0.3f),
            rotationZ = -90,
            sortingOrderOffset = 2,
            flipX = false,
            flipY = false
        };

        public WeaponPositionSettings backAttackSettings = new WeaponPositionSettings
        {
            positionOffset = new Vector2(0, -0.3f),
            rotationZ = 90,
            sortingOrderOffset = -2,
            flipX = false,
            flipY = false
        };

        // Метод для установки дефолтных значений
        public void SetDefaultValues()
        {
            sideRightSettings = new WeaponPositionSettings
            {
                positionOffset = new Vector2(0.5f, 0),
                rotationZ = 0,
                sortingOrderOffset = 1
            };

            sideLeftSettings = new WeaponPositionSettings
            {
                positionOffset = new Vector2(-0.5f, 0),
                rotationZ = 0,
                sortingOrderOffset = -1,
                flipX = true
            };

            frontAttackSettings = new WeaponPositionSettings
            {
                positionOffset = new Vector2(0, 0.3f),
                rotationZ = -90,
                sortingOrderOffset = 2
            };

            backAttackSettings = new WeaponPositionSettings
            {
                positionOffset = new Vector2(0, -0.3f),
                rotationZ = 90,
                sortingOrderOffset = -2
            };
        }
    }

    [System.Serializable]
    public class WeaponPositionSettings
    {
        [Header("Transform")]
        public Vector2 positionOffset;
        public float rotationZ;
        public bool flipX;
        public bool flipY;

        [Header("Rendering")]
        [Tooltip("Offset relative to Arms sorting order")]
        public int sortingOrderOffset = 0;
    }

}