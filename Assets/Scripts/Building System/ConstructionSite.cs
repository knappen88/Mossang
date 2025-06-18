using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

namespace Building
{
    public class ConstructionSite : MonoBehaviour
    {
        [Header("UI Prefabs")]
        [SerializeField] private GameObject constructionUIPrefab;
        [SerializeField] private GameObject progressBarPrefab;

        [Header("Visual Settings")]
        [SerializeField] private float constructionAlpha = 0.5f;
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private Color messageErrorColor = Color.red;
        [SerializeField] private float messageDuration = 2f;

        [Header("Effects")]
        [SerializeField] private bool useParticleEffects = false;
        [SerializeField] private GameObject particleEffectPrefab;

        // Components
        private BuildingData buildingData;
        private Inventory playerInventory;
        private SpriteRenderer[] spriteRenderers;

        // UI Elements
        private GameObject uiInstance;
        private TextMeshProUGUI infoText;
        private Slider progressBar;
        private Transform uiFollowPoint;

        // State
        private bool resourcesDelivered = false;
        private bool isConstructing = false;
        private float constructionProgress = 0f;

        // Cached
        private Transform playerTransform;
        private Camera mainCamera;

        public System.Action<GameObject, BuildingData> OnConstructionComplete;

        #region Initialization

        private void Awake()
        {
            mainCamera = Camera.main;
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        public void Initialize(BuildingData data, Inventory inventory, GameObject uiPrefab = null)
        {
            buildingData = data;
            playerInventory = inventory;

            // Если префаб передан, используем его
            if (uiPrefab != null)
                constructionUIPrefab = uiPrefab;

            // Получаем все рендереры
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

            // Устанавливаем начальную прозрачность
            SetTransparency(constructionAlpha);

            // Создаем UI
            CreateUI();

            // Показываем требования
            UpdateUIText(GetRequirementsText());
        }

        #endregion

        #region UI Management

        private void CreateUI()
        {
            if (constructionUIPrefab == null)
            {
                Debug.LogError("[ConstructionSite] UI Prefab not assigned!");
                return;
            }

            // Создаем экземпляр UI
            uiInstance = Instantiate(constructionUIPrefab);

            // Получаем Canvas компонент
            Canvas canvas = uiInstance.GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ConstructionSite] Canvas component not found in UI prefab!");
                return;
            }

            // ВАЖНО: Настраиваем для World Space
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                // Родитель - само здание
                uiInstance.transform.SetParent(transform);

                // Сбрасываем локальную позицию и поворот
                uiInstance.transform.localPosition = Vector3.zero;
                uiInstance.transform.localRotation = Quaternion.identity;

                // Правильный масштаб для пиксельной графики
                // ВАЖНО: Измени это значение если UI слишком большой/маленький
                float scaleFactor = 1f / 32f; // 32 - это Pixels Per Unit твоих спрайтов
                uiInstance.transform.localScale = Vector3.one * scaleFactor;

                // Позиция над зданием
                float yOffset = buildingData.size.y * 0.5f + 0.5f; // Половина высоты здания + отступ
                uiInstance.transform.localPosition = new Vector3(0, yOffset, -0.1f);

                // Убеждаемся что RectTransform правильно настроен
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                canvasRect.anchorMin = new Vector2(0.5f, 0.5f);
                canvasRect.anchorMax = new Vector2(0.5f, 0.5f);
                canvasRect.pivot = new Vector2(0.5f, 0.5f);
                canvasRect.anchoredPosition = Vector2.zero;
            }

            // Создаем точку следования для UI (опционально)
            GameObject followPoint = new GameObject("UI Follow Point");
            followPoint.transform.SetParent(transform);
            followPoint.transform.localPosition = new Vector3(0, buildingData.size.y * 0.5f + 0.5f, 0);
            uiFollowPoint = followPoint.transform;

            // Находим компоненты
            infoText = uiInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (infoText == null)
            {
                Debug.LogError("[ConstructionSite] TextMeshProUGUI not found in UI prefab!");
            }

            progressBar = uiInstance.GetComponentInChildren<Slider>();
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(false);
            }

            // Отладка позиционирования
            Debug.Log($"[ConstructionSite] Building position: {transform.position}");
            Debug.Log($"[ConstructionSite] Building size: {buildingData.size}");
            Debug.Log($"[ConstructionSite] UI local position: {uiInstance.transform.localPosition}");
            Debug.Log($"[ConstructionSite] UI world position: {uiInstance.transform.position}");
            Debug.Log($"[ConstructionSite] UI scale: {uiInstance.transform.localScale}");

            // Визуальная отладка
            Debug.DrawRay(transform.position, Vector3.up * buildingData.size.y, Color.yellow, 5f);
            Debug.DrawRay(uiInstance.transform.position, Vector3.up * 0.5f, Color.green, 5f);
        }

        private void UpdateUIText(string text, bool isError = false)
        {
            if (infoText == null) return;

            infoText.text = text;
            infoText.color = isError ? messageErrorColor : Color.white;
        }

        private string GetRequirementsText()
        {
            string text = "<b>Требуется:</b>\n";

            foreach (var req in buildingData.resourceRequirements)
            {
                int playerAmount = GetPlayerResourceCount(req.resource);
                bool hasEnough = playerAmount >= req.amount;

                string colorTag = hasEnough ? "<color=green>" : "<color=red>";
                text += $"{colorTag}{req.resource.itemName}: {playerAmount}/{req.amount}</color>\n";
            }

            text += "\n<size=12>Нажмите <color=yellow>F</color> для строительства</size>";
            return text;
        }

        private int GetPlayerResourceCount(ItemData resource)
        {
            if (playerInventory == null) return 0;

            int count = 0;
            foreach (var item in playerInventory.items)
            {
                if (item.itemData == resource)
                    count += item.quantity;
            }
            return count;
        }

        #endregion

        #region Update Loop

        private void Update()
        {
            // Обновляем позицию UI
            UpdateUIPosition();

            // Проверяем ввод
            if (!resourcesDelivered && Input.GetKeyDown(KeyCode.F))
            {
                TryDeliverResources();
            }

            // Обновляем строительство
            if (isConstructing)
            {
                UpdateConstruction();
            }
        }

        private void UpdateUIPosition()
        {
            if (uiInstance == null || mainCamera == null) return;

            Canvas canvas = uiInstance.GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            {
                // Поворачиваем UI к камере
                uiInstance.transform.rotation = mainCamera.transform.rotation;
            }
            else if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Обновляем позицию для Screen Space
                Vector3 screenPos = mainCamera.WorldToScreenPoint(uiFollowPoint.position);
                uiInstance.transform.position = screenPos;
            }
        }

        #endregion

        #region Resource Delivery

        private void TryDeliverResources()
        {
            // Проверяем расстояние до игрока
            if (!IsPlayerNearby())
            {
                ShowTemporaryMessage("Подойдите ближе!", true);
                return;
            }

            // Проверяем ресурсы
            if (!HasRequiredResources())
            {
                ShowTemporaryMessage("Недостаточно ресурсов!", true);
                return;
            }

            // Забираем ресурсы
            ConsumeResources();

            // Начинаем строительство
            StartConstruction();
        }

        private bool IsPlayerNearby()
        {
            if (playerTransform == null) return false;

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            return distance <= interactionDistance;
        }

        private bool HasRequiredResources()
        {
            return buildingData.CanAfford(playerInventory);
        }

        private void ConsumeResources()
        {
            foreach (var requirement in buildingData.resourceRequirements)
            {
                playerInventory.RemoveItem(requirement.resource, requirement.amount);
            }
        }

        #endregion

        #region Construction Process

        private void StartConstruction()
        {
            resourcesDelivered = true;
            isConstructing = true;

            // Обновляем UI
            UpdateUIText("Строительство...");

            // Показываем прогресс бар
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(true);
                progressBar.value = 0;
            }

            // Создаем эффекты
            if (useParticleEffects && particleEffectPrefab != null)
            {
                GameObject effect = Instantiate(particleEffectPrefab, transform);
                effect.transform.localPosition = Vector3.zero;
            }

            // Анимация начала строительства
            transform.DOPunchScale(Vector3.one * 0.1f, 0.5f);
        }

        private void UpdateConstruction()
        {
            // Обновляем прогресс
            constructionProgress += Time.deltaTime / buildingData.buildTime;
            constructionProgress = Mathf.Clamp01(constructionProgress);

            // Обновляем визуал
            if (progressBar != null)
            {
                progressBar.value = constructionProgress;
            }

            // Обновляем прозрачность
            float alpha = Mathf.Lerp(constructionAlpha, 1f, constructionProgress);
            SetTransparency(alpha);

            // Проверяем завершение
            if (constructionProgress >= 1f)
            {
                CompleteConstruction();
            }
        }

        private void CompleteConstruction()
        {
            isConstructing = false;

            // Восстанавливаем полную непрозрачность
            SetTransparency(1f);

            // Удаляем UI
            if (uiInstance != null)
            {
                Destroy(uiInstance);
            }

            // Анимация завершения
            transform.DOPunchScale(Vector3.one * 0.2f, 0.5f)
                .OnComplete(() => {
                    // Вызываем событие завершения
                    OnConstructionComplete?.Invoke(gameObject, buildingData);

                    // Удаляем этот компонент
                    Destroy(this);
                });
        }

        #endregion

        #region Visual Effects

        private void SetTransparency(float alpha)
        {
            foreach (var renderer in spriteRenderers)
            {
                if (renderer != null)
                {
                    Color color = renderer.color;
                    color.a = alpha;
                    renderer.color = color;
                }
            }
        }

        private void ShowTemporaryMessage(string message, bool isError)
        {
            UpdateUIText(message, isError);

            // Возвращаем обычный текст через время
            CancelInvoke(nameof(ResetUIText));
            Invoke(nameof(ResetUIText), messageDuration);
        }

        private void ResetUIText()
        {
            UpdateUIText(GetRequirementsText());
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            // Показываем радиус взаимодействия
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);

            // Показываем позицию UI
            if (uiFollowPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(uiFollowPoint.position, 0.2f);
            }
        }

        #endregion
    }
}