#if UNITY_EDITOR
namespace Combat.Editor
{
    using UnityEngine;
    using UnityEditor;
    using Combat.Weapons;
    using Combat.Core;

    public class WeaponPrefabCreator : EditorWindow
    {
        [MenuItem("Tools/Combat/2D Weapon Prefab Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<WeaponPrefabCreator>("2D Weapon Creator");
            window.minSize = new Vector2(400, 500);
        }

        private string weaponName = "New Weapon";
        private Sprite weaponSprite;
        private Sprite attackSprite;
        private WeaponType weaponType = WeaponType.Sword;
        private bool createSlashEffect = true;
        private bool autoSetupCollider = true;

        private void OnGUI()
        {
            GUILayout.Label("2D Weapon Prefab Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Основные настройки
            weaponName = EditorGUILayout.TextField("Weapon Name", weaponName);
            weaponSprite = EditorGUILayout.ObjectField("Weapon Sprite", weaponSprite, typeof(Sprite), false) as Sprite;
            attackSprite = EditorGUILayout.ObjectField("Attack Sprite (Optional)", attackSprite, typeof(Sprite), false) as Sprite;
            weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weaponType);

            GUILayout.Space(10);
            GUILayout.Label("Components:", EditorStyles.boldLabel);

            createSlashEffect = EditorGUILayout.Toggle("Create Slash Effect", createSlashEffect);
            autoSetupCollider = EditorGUILayout.Toggle("Auto Setup 2D Collider", autoSetupCollider);

            GUILayout.Space(20);

            GUI.enabled = weaponSprite != null;
            if (GUILayout.Button("Create 2D Weapon Prefab", GUILayout.Height(40)))
            {
                Create2DWeaponPrefab();
            }
            GUI.enabled = true;

            GUILayout.Space(10);
            DrawInstructions();
        }

        private void DrawInstructions()
        {
            EditorGUILayout.HelpBox(
                "Инструкция:\n" +
                "1. Выберите спрайт оружия\n" +
                "2. Укажите тип оружия\n" +
                "3. Нажмите 'Create 2D Weapon Prefab'\n" +
                "4. Настройте позицию и коллайдер\n" +
                "5. Сохраните как префаб\n\n" +
                "Оружие будет автоматически менять порядок отрисовки в зависимости от направления персонажа.",
                MessageType.Info
            );
        }

        private void Create2DWeaponPrefab()
        {
            // Создаем корневой объект
            GameObject weaponRoot = new GameObject(weaponName);

            // Добавляем спрайт
            var spriteRenderer = weaponRoot.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = weaponSprite;
            spriteRenderer.sortingLayerName = "Default"; // Измените на ваш слой

            // Добавляем компоненты
            Add2DWeaponComponents(weaponRoot);

            // Позиционируем в сцене
            weaponRoot.transform.position = Vector3.zero;

            // Выделяем в иерархии
            Selection.activeGameObject = weaponRoot;
            EditorGUIUtility.PingObject(weaponRoot);

            Debug.Log($"2D Weapon prefab '{weaponName}' created! Настройте позицию и сохраните как префаб.");
        }

        private void Add2DWeaponComponents(GameObject root)
        {
            // WeaponInstance2D
            var weaponInstance = root.AddComponent<WeaponInstance>();

            // Grip Point
            GameObject gripPoint = new GameObject("GripPoint");
            gripPoint.transform.SetParent(root.transform);
            gripPoint.transform.localPosition = GetDefault2DGripPosition(weaponType);

            // Damage Collider
            if (autoSetupCollider)
            {
                GameObject colliderObj = new GameObject("DamageCollider");
                colliderObj.transform.SetParent(root.transform);
                colliderObj.layer = LayerMask.NameToLayer("Default"); // Измените на ваш слой

                var col = Add2DCollider(colliderObj, weaponType);

                // Настраиваем WeaponInstance2D
                SerializedObject so = new SerializedObject(weaponInstance);
                so.FindProperty("weaponType").enumValueIndex = (int)weaponType;
                so.FindProperty("gripPoint").objectReferenceValue = gripPoint.transform;
                so.FindProperty("damageCollider").objectReferenceValue = col;
                so.FindProperty("idleSprite").objectReferenceValue = weaponSprite;

                if (attackSprite != null)
                {
                    so.FindProperty("attackSprite").objectReferenceValue = attackSprite;
                }

                so.ApplyModifiedProperties();
            }

            // Slash Effect (заготовка)
            if (createSlashEffect)
            {
                Debug.Log("Не забудьте назначить префаб эффекта взмаха в поле Slash Effect Prefab!");
            }
        }

        private Collider2D Add2DCollider(GameObject obj, WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Sword:
                case WeaponType.Dagger:
                    var box = obj.AddComponent<BoxCollider2D>();
                    box.size = new Vector2(0.1f, 0.8f);
                    box.offset = new Vector2(0, 0.4f);
                    return box;

                case WeaponType.Axe:
                case WeaponType.Pickaxe:
                    var axeBox = obj.AddComponent<BoxCollider2D>();
                    axeBox.size = new Vector2(0.4f, 0.3f);
                    axeBox.offset = new Vector2(0, 0.5f);
                    return axeBox;

                case WeaponType.Hammer:
                    var circle = obj.AddComponent<CircleCollider2D>();
                    circle.radius = 0.25f;
                    circle.offset = new Vector2(0, 0.6f);
                    return circle;

                case WeaponType.Staff:
                    var staffCircle = obj.AddComponent<CircleCollider2D>();
                    staffCircle.radius = 0.2f;
                    staffCircle.offset = new Vector2(0, 0.8f);
                    return staffCircle;

                default:
                    return obj.AddComponent<BoxCollider2D>();
            }
        }

        private Vector2 GetDefault2DGripPosition(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Sword:
                case WeaponType.Dagger:
                    return new Vector2(0, -0.3f);
                case WeaponType.Axe:
                case WeaponType.Hammer:
                case WeaponType.Pickaxe:
                    return new Vector2(0, -0.5f);
                case WeaponType.Staff:
                    return new Vector2(0, -0.7f);
                default:
                    return Vector2.zero;
            }
        }
    }
}
#endif