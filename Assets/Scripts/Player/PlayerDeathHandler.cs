using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    private PlayerAnimator animator;
    private PlayerHealth health;
    private bool isDead = false;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        if (health == null)
        {
            Debug.LogError("[PlayerDeathHandler] PlayerHealth component not found!");
            enabled = false;
            return;
        }

        animator = GetComponent<PlayerAnimator>();
        if (animator == null)
        {
            Debug.LogWarning("[PlayerDeathHandler] PlayerAnimator not found. Death animation will not play.");
        }

        health.OnPlayerDied.AddListener(HandleDeath);
    }

    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении
        if (health != null)
        {
            health.OnPlayerDied.RemoveListener(HandleDeath);
        }
    }

    private void HandleDeath()
    {
        // Предотвращаем множественные вызовы
        if (isDead) return;
        isDead = true;

        // Отключаем скрипты
        if (scriptsToDisable != null)
        {
            foreach (var script in scriptsToDisable)
            {
                if (script != null)
                    script.enabled = false;
            }
        }

        // Запускаем анимацию смерти если есть аниматор
        if (animator != null)
        {
            animator.TriggerDeath();
        }

        // Сбрасываем масштаб визуальной части
        if (visualRoot != null)
        {
            visualRoot.transform.localScale = Vector3.one;
        }

        Debug.Log("[PlayerDeathHandler] Player died!");
    }

    // Метод для воскрешения игрока (если понадобится)
    public void Revive()
    {
        if (!isDead) return;

        isDead = false;

        // Включаем скрипты обратно
        if (scriptsToDisable != null)
        {
            foreach (var script in scriptsToDisable)
            {
                if (script != null)
                    script.enabled = true;
            }
        }

        Debug.Log("[PlayerDeathHandler] Player revived!");
    }
}