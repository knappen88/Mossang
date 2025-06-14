using UnityEngine;
using DG.Tweening;

public class InventoryToggleUI : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;

    [Header("References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private CanvasGroup inventoryCanvasGroup;
    [SerializeField] private RectTransform inventoryPanelTransform;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Tween currentTween;

    private void Awake()
    {
        ValidateReferences();
    }

    private void Start()
    {
        // Убеждаемся что инвентарь закрыт при старте
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isOpen = false;
        }

        // Устанавливаем начальные значения
        if (inventoryCanvasGroup != null)
        {
            inventoryCanvasGroup.alpha = 0f;
            inventoryCanvasGroup.interactable = false;
            inventoryCanvasGroup.blocksRaycasts = false;
        }

        if (inventoryPanelTransform != null)
        {
            inventoryPanelTransform.localScale = Vector3.zero;
        }
    }

    private void ValidateReferences()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("[InventoryToggleUI] Inventory Panel не назначена!");
            enabled = false;
            return;
        }

        // Пытаемся получить компоненты автоматически если не назначены
        if (inventoryCanvasGroup == null)
        {
            inventoryCanvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
            if (inventoryCanvasGroup == null)
            {
                inventoryCanvasGroup = inventoryPanel.AddComponent<CanvasGroup>();
            }
        }

        if (inventoryPanelTransform == null)
        {
            inventoryPanelTransform = inventoryPanel.GetComponent<RectTransform>();
        }

        // Проверяем CanvasGroup на родителе
        var parentCanvasGroup = GetComponent<CanvasGroup>();
        if (parentCanvasGroup != null)
        {
            Debug.LogWarning("[InventoryToggleUI] CanvasGroup найден на родителе. Это может влиять на видимость!");
            parentCanvasGroup.alpha = 1f;
            parentCanvasGroup.interactable = true;
            parentCanvasGroup.blocksRaycasts = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isAnimating)
        {
            Toggle();
        }

        // Быстрое закрытие на Escape
        if (isOpen && Input.GetKeyDown(KeyCode.Escape) && !isAnimating)
        {
            Close();
        }
    }

    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (isAnimating || inventoryPanel == null || isOpen) return;

        // Останавливаем предыдущую анимацию
        KillCurrentTween();

        isAnimating = true;
        isOpen = true;

        // Активируем панель
        inventoryPanel.SetActive(true);

        // Блокируем движение игрока
        var playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.DisableMovement();
        }

        // Создаем анимацию открытия
        currentTween = DOTween.Sequence()
            .Append(inventoryCanvasGroup.DOFade(1f, animationDuration * 0.8f))
            .Join(inventoryPanelTransform.DOScale(1f, animationDuration).SetEase(openEase))
            .OnComplete(() => {
                isAnimating = false;
                inventoryCanvasGroup.interactable = true;
                inventoryCanvasGroup.blocksRaycasts = true;
            })
            .SetUpdate(true); // Работает даже при паузе
    }

    public void Close()
    {
        if (isAnimating || inventoryPanel == null || !isOpen) return;

        // Останавливаем предыдущую анимацию
        KillCurrentTween();

        isAnimating = true;
        isOpen = false;

        // Блокируем взаимодействие
        inventoryCanvasGroup.interactable = false;
        inventoryCanvasGroup.blocksRaycasts = false;

        // Разблокируем движение игрока
        var playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.EnableMovement();
        }

        // Создаем анимацию закрытия
        currentTween = DOTween.Sequence()
            .Append(inventoryPanelTransform.DOScale(0f, animationDuration).SetEase(closeEase))
            .Join(inventoryCanvasGroup.DOFade(0f, animationDuration * 0.8f))
            .OnComplete(() => {
                isAnimating = false;
                inventoryPanel.SetActive(false);
            })
            .SetUpdate(true);
    }

    private void KillCurrentTween()
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill(false);
            currentTween = null;
        }
    }

    private void OnDestroy()
    {
        KillCurrentTween();
    }

    // Публичные методы для получения состояния
    public bool IsOpen => isOpen;
    public bool IsAnimating => isAnimating;
}