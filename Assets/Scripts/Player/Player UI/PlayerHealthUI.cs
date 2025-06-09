using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image damageBarFill; // Красная полоска под основной
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float damageBarDelay = 0.5f;

    private Sequence healthBarSequence;

    private void OnEnable()
    {
        playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
        playerHealth.OnDamageTaken.AddListener(OnDamageTaken);
    }

    private void OnDisable()
    {
        playerHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
        playerHealth.OnDamageTaken.RemoveListener(OnDamageTaken);
    }

    private void UpdateHealthBar(int current, int max)
    {
        float fillAmount = (float)current / max;

        // Анимируем основную полоску здоровья
        healthBarFill.DOFillAmount(fillAmount, animationDuration)
            .SetEase(Ease.OutQuad);

        // Анимируем полоску урона с задержкой
        if (damageBarFill != null)
        {
            DOTween.Sequence()
                .AppendInterval(damageBarDelay)
                .Append(damageBarFill.DOFillAmount(fillAmount, animationDuration * 2)
                    .SetEase(Ease.InOutQuad));
        }
    }

    private void OnDamageTaken(int damage, Vector2 sourcePosition)
    {
        // Встряска UI при получении урона
        transform.DOShakePosition(0.2f, 10f, 20)
            .SetRelative(true);

        // Пульсация полоски здоровья
        healthBarFill.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5);
    }
}