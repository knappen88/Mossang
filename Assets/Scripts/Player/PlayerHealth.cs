using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    public int CurrentHealth { get; private set; }

    public UnityEvent<int, int> OnHealthChanged; // (current, max)
    public UnityEvent<int, Vector2> OnDamageTaken; // (damage, sourcePosition)
    public UnityEvent OnPlayerDied;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool enableTestDamage = true;

    private void Update()
    {
        // Тестовый урон только в редакторе
        if (enableTestDamage && Input.GetKeyDown(GameConstants.KEY_TEST_DAMAGE))
        {
            TakeDamage(GameConstants.TEST_DAMAGE_AMOUNT, transform.position + Vector3.left);
        }
    }
#endif

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Start()
    {
        // Отправляем начальное состояние здоровья
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int amount, Vector2 damageSourcePosition)
    {
        if (CurrentHealth <= 0) return;
        if (amount <= 0) return;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnDamageTaken?.Invoke(amount, damageSourcePosition);

        if (CurrentHealth == 0)
        {
            Die();
        }
    }

    public void TakeDamage(int amount)
    {
        // Вызываем основной метод с позицией игрока как источником
        TakeDamage(amount, transform.position);
    }

    public void Heal(int amount)
    {
        if (CurrentHealth <= 0) return; // Нельзя лечить мертвого
        if (amount <= 0) return;

        int oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);

        if (CurrentHealth != oldHealth)
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            Debug.Log($"[PlayerHealth] Healed for {CurrentHealth - oldHealth} HP");
        }
    }

    public void SetMaxHealth(int newMaxHealth, bool healToFull = false)
    {
        if (newMaxHealth <= 0) return;

        maxHealth = newMaxHealth;

        if (healToFull)
        {
            CurrentHealth = maxHealth;
        }
        else
        {
            CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        }

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log("[PlayerHealth] Player died!");
        OnPlayerDied?.Invoke();
    }

    // Методы для получения информации о здоровье
    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? (float)CurrentHealth / maxHealth : 0f;
    }

    public bool IsAlive()
    {
        return CurrentHealth > 0;
    }

    public bool IsFullHealth()
    {
        return CurrentHealth >= maxHealth;
    }
}