using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthBarFill;

    private void OnEnable()
    {
        playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
    }

    private void OnDisable()
    {
        playerHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
    }

    private void UpdateHealthBar(int current, int max)
    {
        float fillAmount = (float)current / max;
        healthBarFill.fillAmount = fillAmount;
    }
}
