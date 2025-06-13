using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Items
{
    /// <summary>
    /// Компонент для префаба подсказки подбора
    /// </summary>
    public class PickupHintPrefab : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI actionText;
        [SerializeField] private Image keyBackground;
        [SerializeField] private TextMeshProUGUI keyText;

        [Header("Style Settings")]
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
        [SerializeField] private Color keyColor = new Color(1, 1, 1, 0.9f);
        [SerializeField] private Color textColor = Color.white;

        private void Start()
        {
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            if (actionText != null)
            {
                actionText.text = "to pickup";
                actionText.color = textColor;
            }

            if (keyText != null)
            {
                keyText.text = "E";
                keyText.color = Color.black;
            }

            if (keyBackground != null)
            {
                keyBackground.color = keyColor;
            }
        }

        public void SetKey(string key)
        {
            if (keyText != null)
                keyText.text = key;
        }

        public void SetActionText(string action)
        {
            if (actionText != null)
                actionText.text = action;
        }
    }
}