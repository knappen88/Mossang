using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    public int CurrentHealth { get; private set; }

    public UnityEvent<int, int> OnHealthChanged; // (current, max)
    public UnityEvent<int, Vector2> OnDamageTaken; // (damage, sourcePosition)
    public UnityEvent OnPlayerDied;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            TakeDamage(10, transform.position + Vector3.left); // Урон слева для теста
    }

    public void TakeDamage(int amount, Vector2 damageSourcePosition)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnDamageTaken?.Invoke(amount, damageSourcePosition);

        if (CurrentHealth == 0)
        {
            OnPlayerDied?.Invoke();
        }
    }

    // Перегрузка для обратной совместимости
    public void TakeDamage(int amount)
    {
        TakeDamage(amount, transform.position);
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}