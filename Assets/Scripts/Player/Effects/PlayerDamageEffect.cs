using System.Collections;
using DG.Tweening;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerDamageEffect : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private SpriteRenderer[] spriteRenderers; // Все спрайты персонажа
    [SerializeField] private Material flashMaterial; // Материал для вспышки (белый)

    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private int flashCount = 3;
    [SerializeField] private Color damageColor = new Color(1f, 0.3f, 0.3f, 1f);

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeStrength = 0.2f;
    [SerializeField] private int shakeVibrato = 20;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 2f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Additional Effects")]
    [SerializeField] private GameObject damageNumberPrefab; // Префаб для показа урона
    [SerializeField] private ParticleSystem bloodParticles; // Частицы крови
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] hurtSounds; // Массив звуков получения урона

    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private Material[] originalMaterials;
    private bool isInvulnerable = false;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();

        // Сохраняем оригинальные материалы
        originalMaterials = new Material[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalMaterials[i] = spriteRenderers[i].material;
        }
    }

    private void OnEnable()
    {
        playerHealth.OnDamageTaken.AddListener(OnDamageTaken);
    }

    private void OnDisable()
    {
        playerHealth.OnDamageTaken.RemoveListener(OnDamageTaken);
    }

    private void OnDamageTaken(int damage, Vector2 damageSourcePosition)
    {
        if (!isInvulnerable)
        {
            StartCoroutine(DamageEffectSequence(damage, damageSourcePosition));
        }
    }

    private IEnumerator DamageEffectSequence(int damage, Vector2 damageSourcePosition)
    {
        isInvulnerable = true;

        // 1. Воспроизводим звук
        PlayHurtSound();

        // 2. Показываем число урона
        ShowDamageNumber(damage);

        // 3. Создаем частицы крови
        if (bloodParticles != null)
        {
            bloodParticles.Play();
        }

        // 4. Эффект отбрасывания
        ApplyKnockback(damageSourcePosition);

        // 5. Встряска камеры (если есть CameraShake компонент)
        CameraShake.Instance?.Shake(shakeDuration, shakeStrength);

        // 6. Визуальные эффекты на персонаже
        StartCoroutine(FlashEffect());
        ShakeCharacter();

        // 7. Период неуязвимости с миганием
        yield return StartCoroutine(InvulnerabilityBlink(1.5f));

        isInvulnerable = false;
    }

    private void PlayHurtSound()
    {
        if (audioSource != null && hurtSounds.Length > 0)
        {
            AudioClip randomClip = hurtSounds[Random.Range(0, hurtSounds.Length)];
            audioSource.PlayOneShot(randomClip);
        }
    }

    private void ShowDamageNumber(int damage)
    {
        if (damageNumberPrefab != null)
        {
            GameObject damageNumber = Instantiate(damageNumberPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            DamageNumber dnComponent = damageNumber.GetComponent<DamageNumber>();
            if (dnComponent != null)
            {
                dnComponent.SetDamage(damage);
            }
        }
    }

    private void ApplyKnockback(Vector2 damageSourcePosition)
    {
        if (playerMovement != null)
        {
            Vector2 knockbackDirection = ((Vector2)transform.position - damageSourcePosition).normalized;

            playerMovement.DisableMovement();

            // Используем DOTween для плавного отбрасывания
            transform.DOMove(transform.position + (Vector3)(knockbackDirection * knockbackForce), knockbackDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => playerMovement.EnableMovement());
        }
    }

    private IEnumerator FlashEffect()
    {
        // Меняем материал на белый
        if (flashMaterial != null)
        {
            SetMaterial(flashMaterial);
            yield return new WaitForSeconds(flashDuration);
            RestoreOriginalMaterials();
        }

        // Цветовая вспышка
        for (int i = 0; i < flashCount; i++)
        {
            SetColor(damageColor);
            yield return new WaitForSeconds(flashDuration);
            SetColor(Color.white);
            yield return new WaitForSeconds(flashDuration);
        }
    }

    private void ShakeCharacter()
    {
        if (visualTransform != null)
        {
            visualTransform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato)
                .SetRelative(true);
        }
    }

    private IEnumerator InvulnerabilityBlink(float duration)
    {
        float elapsed = 0f;
        float blinkInterval = 0.1f;

        while (elapsed < duration)
        {
            SetAlpha(0.3f);
            yield return new WaitForSeconds(blinkInterval);
            SetAlpha(1f);
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval * 2;
        }

        SetAlpha(1f);
    }

    private void SetMaterial(Material material)
    {
        foreach (var renderer in spriteRenderers)
        {
            renderer.material = material;
        }
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].material = originalMaterials[i];
        }
    }

    private void SetColor(Color color)
    {
        foreach (var renderer in spriteRenderers)
        {
            renderer.color = color;
        }
    }

    private void SetAlpha(float alpha)
    {
        foreach (var renderer in spriteRenderers)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }
}