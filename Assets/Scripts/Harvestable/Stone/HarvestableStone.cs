// ===== Harvestable/Stone/HarvestableStone.cs =====
using UnityEngine;
using System.Collections;
using DG.Tweening;
using Items;

public class HarvestableStone : MonoBehaviour, IHarvestable
{
    [Header("Stone Settings")]
    [SerializeField] private int maxHealth = 150;
    [SerializeField] private ResourceType resourceType = ResourceType.Stone;

    [Header("Loot Settings")]
    [SerializeField] private ItemData stoneResource;
    [SerializeField] private ItemData oreResource; // Опциональная руда
    [SerializeField] private int minStoneDrop = 2;
    [SerializeField] private int maxStoneDrop = 5;
    [SerializeField] private float oreDropChance = 0.2f; // 20% шанс выпадения руды
    [SerializeField] private int minOreDrop = 1;
    [SerializeField] private int maxOreDrop = 2;
    [SerializeField] private float dropForce = 2.5f;
    [SerializeField] private float dropRadius = 0.8f;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject destroyEffectPrefab;
    [SerializeField] private float shakeIntensity = 0.05f;
    [SerializeField] private int shakeVibrato = 20;
    [SerializeField] private Color damageColor = new Color(0.8f, 0.8f, 0.8f);

    [Header("Audio")]
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private AudioSource audioSource;

    private int currentHealth;
    private bool isBeingHarvested = false;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Color originalColor;
    private ItemSpawner itemSpawner;

    // Реализация интерфейса IHarvestable
    public bool IsDestroyed => currentHealth <= 0;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.position;
        originalScale = transform.localScale;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

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
        if (tool.toolType != ToolType.Pickaxe) return false;

        // Проверяем, может ли инструмент добывать этот тип ресурса
        foreach (var resource in tool.gatherableResources)
        {
            if (resource == resourceType || resource == ResourceType.Ore)
                return true;
        }

        return false;
    }

    // Реализация IHarvestable.GetResourceType
    public ResourceType GetResourceType()
    {
        return resourceType;
    }

    // Основной метод получения урона
    private void TakeDamage(int damage, GameObject harvester)
    {
        if (IsDestroyed || isBeingHarvested) return;

        currentHealth -= damage;

        // Визуальная обратная связь
        ShowHitFeedback(harvester.transform.position);

        // Звуковая обратная связь
        PlayHitSound();

        if (currentHealth <= 0)
        {
            StartCoroutine(DestroyStone(harvester));
        }
    }

    private void ShowHitFeedback(Vector3 hitPosition)
    {
        // Эффект попадания
        if (hitEffectPrefab != null)
        {
            // Определяем направление удара для 2D
            Vector2 direction = ((Vector2)transform.position - (Vector2)hitPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            var effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.Euler(0, 0, angle));
            Destroy(effect, 2f);
        }

        // Встряска камня
        transform.DOComplete();
        transform.DOShakePosition(0.2f, shakeIntensity, shakeVibrato)
            .SetEase(Ease.OutQuad);

        // Изменение цвета при ударе
        if (spriteRenderer != null)
        {
            spriteRenderer.DOComplete();
            spriteRenderer.DOColor(damageColor, 0.1f)
                .OnComplete(() => spriteRenderer.DOColor(originalColor, 0.1f));
        }

        // Небольшое уменьшение размера для эффекта "откалывания"
        float healthPercent = (float)currentHealth / maxHealth;
        transform.DOScale(originalScale * (0.8f + healthPercent * 0.2f), 0.1f);
    }

    private void ShowWrongToolFeedback()
    {
        // Легкая встряска без урона
        transform.DOComplete();
        transform.DOShakePosition(0.1f, shakeIntensity * 0.5f, 5)
            .SetEase(Ease.OutQuad);

        // Красный оттенок для обозначения неправильного инструмента
        if (spriteRenderer != null)
        {
            spriteRenderer.DOColor(Color.red, 0.1f)
                .OnComplete(() => spriteRenderer.DOColor(originalColor, 0.1f));
        }

        Debug.Log("Нужна кирка для добычи камня!");
    }

    private IEnumerator DestroyStone(GameObject harvester)
    {
        isBeingHarvested = true;

        // Эффект разрушения
        if (destroyEffectPrefab != null)
        {
            var effect = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 5f);
        }

        // Звук разрушения
        PlayBreakSound();

        // Анимация разрушения для 2D
        if (spriteRenderer != null)
        {
            // Затухание
            spriteRenderer.DOFade(0.3f, 0.2f);
        }

        // Разлет на части
        transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack);

        transform.DORotate(new Vector3(0, 0, Random.Range(-45f, 45f)), 0.3f, RotateMode.FastBeyond360);

        // Спавним ресурсы
        yield return new WaitForSeconds(0.15f);
        SpawnResources();

        // Ждем завершения анимации
        yield return new WaitForSeconds(0.15f);

        // Уничтожаем объект
        Destroy(gameObject);
    }

    private void SpawnResources()
    {
        // Спавним камни
        if (stoneResource != null)
        {
            int stoneAmount = Random.Range(minStoneDrop, maxStoneDrop + 1);
            SpawnItems(stoneResource, stoneAmount);
        }

        // Шанс спавна руды
        if (oreResource != null && Random.value < oreDropChance)
        {
            int oreAmount = Random.Range(minOreDrop, maxOreDrop + 1);
            SpawnItems(oreResource, oreAmount);
        }
    }

    private void SpawnItems(ItemData item, int amount)
    {
        if (itemSpawner != null)
        {
            // Спавним предметы по одному в случайных позициях
            for (int i = 0; i < amount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * dropRadius;
                Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

                // Используем публичный метод SpawnItem
                itemSpawner.SpawnItem(item, 1, spawnPos);
            }
        }
        else
        {
            // Резервный метод если нет ItemSpawner
            Debug.LogWarning($"ItemSpawner not found on {gameObject.name}. Cannot spawn {item.itemName}");
        }
    }

    private void PlayHitSound()
    {
        if (hitSounds != null && hitSounds.Length > 0 && audioSource != null)
        {
            var randomSound = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(randomSound, 0.8f);
        }
    }

    private void PlayBreakSound()
    {
        if (breakSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(breakSound, 1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация радиуса выпадения ресурсов для 2D
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, dropRadius);
    }
}


/*
 эффекты:

1. StoneHitEffect_2D:
   - Particle System с Renderer Mode: Billboard
   - Texture: маленькие осколки камня
   - Simulation Space: World
   - Emission: Burst 5-10 частиц
   - Start Speed: 2-4
   - Gravity Modifier: 0.5
   - Start Lifetime: 0.5-1

2. StoneDestroyEffect_2D:
   - Более крупные осколки
   - Burst 15-25 частиц
   - Больший разброс скорости
   - Можно добавить пыль
*/