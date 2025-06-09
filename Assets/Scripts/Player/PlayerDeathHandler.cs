using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private MonoBehaviour[] scriptsToDisable;
    private PlayerAnimator animator;

    private PlayerHealth health;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        animator = GetComponent<PlayerAnimator>();
        health.OnPlayerDied.AddListener(HandleDeath);
    }

    private void HandleDeath()
    {
        foreach (var script in scriptsToDisable)
            script.enabled = false;

        animator.TriggerDeath();

        if (visualRoot != null)
            visualRoot.transform.localScale = Vector3.one;
    }
}
