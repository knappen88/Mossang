using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 originalPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            originalPosition = transform.localPosition;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Shake(float duration, float strength)
    {
        transform.DOShakePosition(duration, strength, 10, 90, false, true)
            .OnComplete(() => transform.localPosition = originalPosition);
    }
}