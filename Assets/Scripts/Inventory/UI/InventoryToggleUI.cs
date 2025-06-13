using UnityEngine;
using DG.Tweening;

public class InventoryToggleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject inventoryPanel; // Только панель инвентаря, без хотбара
    [SerializeField] private CanvasGroup inventoryCanvasGroup; // CanvasGroup именно для inventoryPanel
    [SerializeField] private RectTransform inventoryPanelTransform; // RectTransform именно для inventoryPanel

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Tween currentTween;

    private void Start()
    {
        Debug.Log($"[InventoryToggleUI] Start - inventoryPanel = {inventoryPanel?.name}");

        // ВАЖНО: Получаем компоненты именно с inventoryPanel, а не с текущего GameObject
        if (inventoryPanel != null)
        {
            // Создаем CanvasGroup на inventoryPanel если его нет
            if (inventoryCanvasGroup == null)
            {
                inventoryCanvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
                if (inventoryCanvasGroup == null)
                {
                    inventoryCanvasGroup = inventoryPanel.AddComponent<CanvasGroup>();
                }
            }

            // Получаем RectTransform inventoryPanel
            if (inventoryPanelTransform == null)
            {
                inventoryPanelTransform = inventoryPanel.GetComponent<RectTransform>();
            }

            // Начальное состояние - скрываем ТОЛЬКО панель инвентаря
            inventoryPanel.SetActive(false);
            inventoryCanvasGroup.alpha = 0f;
            inventoryPanelTransform.localScale = Vector3.zero;

            Debug.Log($"[InventoryToggleUI] Инициализация завершена. Скрыт только: {inventoryPanel.name}");
        }

        // Проверяем что у нас самих нет CanvasGroup который может влиять на детей
        var myCanvasGroup = GetComponent<CanvasGroup>();
        if (myCanvasGroup != null)
        {
            Debug.LogWarning($"[InventoryToggleUI] На {gameObject.name} есть CanvasGroup! Это может влиять на видимость детей!");
            myCanvasGroup.alpha = 1f;
            myCanvasGroup.interactable = true;
            myCanvasGroup.blocksRaycasts = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isAnimating)
        {
            if (isOpen)
                Close();
            else
                Open();
        }

        // Быстрое закрытие на Escape
        if (isOpen && Input.GetKeyDown(KeyCode.Escape) && !isAnimating)
        {
            Close();
        }
    }

    private void Open()
    {
        if (isAnimating || inventoryPanel == null) return;

        isAnimating = true;
        isOpen = true;

        // Активируем ТОЛЬКО панель инвентаря
        inventoryPanel.SetActive(true);

        // Блокируем движение игрока
        var playerMovement = FindObjectOfType<PlayerMovement>();
        playerMovement?.DisableMovement();

        // Создаем анимацию используя компоненты inventoryPanel
        currentTween = DOTween.Sequence()
            .Append(inventoryCanvasGroup.DOFade(1f, animationDuration * 0.8f))
            .Join(inventoryPanelTransform.DOScale(1f, animationDuration).SetEase(openEase))
            .OnComplete(() => {
                isAnimating = false;
                inventoryCanvasGroup.interactable = true;
                inventoryCanvasGroup.blocksRaycasts = true;
            });
    }

    private void Close()
    {
        if (isAnimating || inventoryPanel == null) return;

        isAnimating = true;
        isOpen = false;

        // Блокируем взаимодействие
        inventoryCanvasGroup.interactable = false;
        inventoryCanvasGroup.blocksRaycasts = false;

        // Разблокируем движение игрока
        var playerMovement = FindObjectOfType<PlayerMovement>();
        playerMovement?.EnableMovement();

        // Создаем анимацию
        currentTween = DOTween.Sequence()
            .Append(inventoryPanelTransform.DOScale(0f, animationDuration).SetEase(closeEase))
            .Join(inventoryCanvasGroup.DOFade(0f, animationDuration * 0.8f))
            .OnComplete(() => {
                isAnimating = false;
                inventoryPanel.SetActive(false);
            });
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
    }
}