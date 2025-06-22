using UnityEngine;

namespace BuildingSystem.Config
{
    [CreateAssetMenu(fileName = "BuildingSystemConfig", menuName = "Building System/System Config")]
    public class BuildingSystemConfig : ScriptableObject
    {
        [Header("Input Settings")]
        [SerializeField] private KeyCode buildMenuKey = KeyCode.B;
        [SerializeField] private KeyCode confirmPlacementKey = KeyCode.F;
        [SerializeField] private KeyCode cancelPlacementKey = KeyCode.Escape;
        [SerializeField] private KeyCode rotateKey = KeyCode.R;
        [SerializeField] private KeyCode demolishKey = KeyCode.Delete;

        [Header("Visual Settings")]
        [SerializeField] private bool showGridOnPlacement = true;
        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private float gridLineWidth = 0.02f;
        [SerializeField] private int gridVisibilityRadius = 10;

        [Header("Ghost Settings")]
        [SerializeField] private Material ghostMaterial;
        [SerializeField] private float ghostValidAlpha = 0.6f;
        [SerializeField] private float ghostInvalidAlpha = 0.3f;
        [SerializeField] private Color validPlacementTint = new Color(0, 1, 0, 0.5f);
        [SerializeField] private Color invalidPlacementTint = new Color(1, 0, 0, 0.5f);

        [Header("Placement Settings")]
        [SerializeField] private LayerMask placementLayerMask = -1;
        [SerializeField] private LayerMask obstacleLayerMask = -1;
        [SerializeField] private float placementRaycastDistance = 100f;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip placementSound;
        [SerializeField] private AudioClip cancelSound;
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private AudioClip constructionCompleteSound;
        [SerializeField] private float audioVolume = 1f;

        [Header("UI Settings")]
        [SerializeField] private float uiAnimationDuration = 0.3f;
        [SerializeField] private AnimationCurve uiAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Properties
        public KeyCode BuildMenuKey => buildMenuKey;
        public KeyCode ConfirmPlacementKey => confirmPlacementKey;
        public KeyCode CancelPlacementKey => cancelPlacementKey;
        public KeyCode RotateKey => rotateKey;
        public KeyCode DemolishKey => demolishKey;

        public bool ShowGridOnPlacement => showGridOnPlacement;
        public Color GridColor => gridColor;
        public float GridLineWidth => gridLineWidth;
        public int GridVisibilityRadius => gridVisibilityRadius;

        public Material GhostMaterial => ghostMaterial;
        public float GhostValidAlpha => ghostValidAlpha;
        public float GhostInvalidAlpha => ghostInvalidAlpha;
        public Color ValidPlacementTint => validPlacementTint;
        public Color InvalidPlacementTint => invalidPlacementTint;

        public LayerMask PlacementLayerMask => placementLayerMask;
        public LayerMask ObstacleLayerMask => obstacleLayerMask;
        public float PlacementRaycastDistance => placementRaycastDistance;

        public AudioClip PlacementSound => placementSound;
        public AudioClip CancelSound => cancelSound;
        public AudioClip ErrorSound => errorSound;
        public AudioClip ConstructionCompleteSound => constructionCompleteSound;
        public float AudioVolume => audioVolume;

        public float UIAnimationDuration => uiAnimationDuration;
        public AnimationCurve UIAnimationCurve => uiAnimationCurve;
    }
}