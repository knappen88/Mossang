using UnityEngine;
using System.Collections;
using DG.Tweening;
using Items;

public class HarvestableTree : MonoBehaviour, IHarvestable
{
    [Header("Tree Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private ResourceType resourceType = ResourceType.Wood;

    [Header("Loot Settings")]
    [SerializeField] private ItemData woodResource;
    [SerializeField] private int minWoodDrop = 3;
    [SerializeField] private int maxWoodDrop = 6;
    [SerializeField] private float dropForce = 3f;
    [SerializeField] private float dropRadius = 1f;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject destroyEffectPrefab;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private int shakeVibrato = 10;

    [Header("Audio")]
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private AudioClip fallSound;
    [SerializeField] private AudioSource audioSource;

    private int currentHealth;
    private bool isBeingHarvested = false;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition;
    private ItemSpawner itemSpawner;

    // Реализация интерфейса IHarvestable
    public bool IsDestroyed => currentHealth <= 0;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.position;

        // Получаем или создаем ItemSpawner
        itemSpawner = GetComponent<ItemSpawner>();
        if (itemSpawner == null)
        {
            itemSpawner = gameObject.AddComponent<ItemSpawner>();
        }

        // Если нет AudioSource, создаем
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Реализация IHarvestable.Harvest
    public void Harvest(ToolData tool, GameObject harvester)
    {
        if (!CanBeHarvestedWith(tool))
        {
            ShowWrongToolFeedback();
            return;
        }

        // Применяем урон с учетом эффективности инструмента
        int damage = Mathf.RoundToInt(tool.damagePerUse * tool.efficiency);
        TakeDamage(damage, harvester);
    }

    // Реализация IHarvestable.CanBeHarvestedWith
    public bool CanBeHarvestedWith(ToolData tool)
    {
        if (tool == null) return false;

        // Проверяем тип инструмента
        if (tool.toolType != ToolType.Axe) return false;

        // Проверяем, может ли инструмент добывать этот тип ресурса
        foreach (var resource in tool.gatherableResources)
        {
            if (resource == resourceType)
                return true;
        }

        return false;
    }

    // Реализация IHarvestable.GetResourceType
    public ResourceType GetResourceType()
    {
        return resourceType;
    }

    // Основной метод получения урона (теперь приватный)
    private void TakeDamage(int damage, GameObject attacker)
    {
        if (isBeingHarvested || IsDestroyed) return;

        currentHealth -= damage;

        // Визуальная обратная связь
        ShowHitFeedback();

        // Проверяем, уничтожено ли дерево
        if (currentHealth <= 0)
        {
            StartCoroutine(DestroyTree());
        }
    }

    private void ShowHitFeedback()
    {
        // Звук удара
        if (hitSounds.Length > 0)
        {
            var randomSound = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(randomSound);
        }

        // Эффект удара
        if (hitEffectPrefab != null)
        {
            var effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // Тряска дерева
        transform.DOShakePosition(0.2f, shakeIntensity, shakeVibrato)
            .OnComplete(() => transform.position = originalPosition);

        // Изменение цвета
        if (spriteRenderer != null)
        {
            spriteRenderer.DOColor(Color.red, 0.1f)
                .OnComplete(() => spriteRenderer.DOColor(Color.white, 0.1f));
        }

        // UI индикатор здоровья (опционально)
        ShowHealthBar();
    }

    private void ShowWrongToolFeedback()
    {
        // Легкая тряска для обозначения неправильного инструмента
        transform.DOShakePosition(0.1f, 0.02f, 5);

        // Можно добавить UI подсказку
        Debug.Log("You need an axe to chop this tree!");

        // Звук неправильного инструмента
        // audioSource.PlayOneShot(wrongToolSound);
    }

    private IEnumerator DestroyTree()
    {
        isBeingHarvested = true;

        // Звук падения
        if (fallSound != null)
        {
            audioSource.PlayOneShot(fallSound);
        }

        // Анимация падения
        transform.DORotate(new Vector3(0, 0, 90), 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.InQuad);

        yield return transform.DOScale(Vector3.zero, 1f)
            .SetEase(Ease.InBack)
            .WaitForCompletion();

        // Спавним дроп
        SpawnLoot();

        // Эффект уничтожения
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }

        // Уничтожаем дерево
        Destroy(gameObject);
    }

    private void SpawnLoot()
    {
        if (woodResource == null)
        {
            Debug.LogError("Wood resource not assigned!");
            return;
        }

        int woodAmount = Random.Range(minWoodDrop, maxWoodDrop + 1);

        for (int i = 0; i < woodAmount; i++)
        {
            // Случайная позиция в радиусе
            Vector2 randomOffset = Random.insideUnitCircle * dropRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

            // Спавним через ItemSpawner
            itemSpawner.SpawnItem(woodResource, 1, spawnPos);
        }
    }

    private void ShowHealthBar()
    {
        // Здесь можно показать полоску здоровья над деревом
        float healthPercent = (float)currentHealth / maxHealth;
        Debug.Log($"Tree health: {healthPercent * 100}%");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, dropRadius);
    }

    // Публичные методы для внешнего использования
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    public void SetItemSpawnerPrefab(GameObject pickupPrefab)
    {
        if (itemSpawner != null)
        {
            // Устанавливаем префаб для ItemSpawner
            var prefabField = itemSpawner.GetType().GetField("pickupItemPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prefabField != null)
            {
                prefabField.SetValue(itemSpawner, pickupPrefab);
            }
        }
    }
}