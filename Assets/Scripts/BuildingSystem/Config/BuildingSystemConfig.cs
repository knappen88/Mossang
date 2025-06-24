using UnityEngine;

namespace BuildingSystem.Config
{
    [CreateAssetMenu(fileName = "BuildingSystemConfig", menuName = "Building System/System Config")]
    public class BuildingSystemConfig : ScriptableObject
    {
        [Header("Input Settings")]
        [SerializeField] private KeyCode buildMenuKey = KeyCode.B;
        [SerializeField] private KeyCode cancelPlacementKey = KeyCode.Escape;
        [SerializeField] private KeyCode confirmPlacementKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode rotateKey = KeyCode.R;
        [SerializeField] private KeyCode demolishKey = KeyCode.X;

        [Header("Visual Settings")]
        [SerializeField] private bool showGridOnPlacement = true;
        [SerializeField] private Color validPlacementColor = new Color(0, 1, 0, 0.5f);
        [SerializeField] private Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);

        [Header("Building Pool Settings")]
        [SerializeField] private int defaultPoolSize = 10;
        [SerializeField] private int maxPoolSize = 50;

        [Header("Construction Settings")]
        [SerializeField] private bool instantConstruction = false;
        [SerializeField] private float constructionSpeedMultiplier = 1f;

        // Properties
        public KeyCode BuildMenuKey => buildMenuKey;
        public KeyCode CancelPlacementKey => cancelPlacementKey;
        public KeyCode ConfirmPlacementKey => confirmPlacementKey;
        public KeyCode RotateKey => rotateKey;
        public KeyCode DemolishKey => demolishKey;

        public bool ShowGridOnPlacement => showGridOnPlacement;
        public Color ValidPlacementColor => validPlacementColor;
        public Color InvalidPlacementColor => invalidPlacementColor;

        public int DefaultPoolSize => defaultPoolSize;
        public int MaxPoolSize => maxPoolSize;

        public bool InstantConstruction => instantConstruction;
        public float ConstructionSpeedMultiplier => constructionSpeedMultiplier;
    }
}