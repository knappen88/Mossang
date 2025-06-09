using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageNumber : MonoBehaviour
{
    
    [SerializeField] private TextMeshPro damageTextWorld; // Для мирового пространства
    [SerializeField] private float riseDuration = 1f;
    [SerializeField] private float riseHeight = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private void Awake()
    {
        // Автоматически находим нужный компонент
        
        if (damageTextWorld == null)
            damageTextWorld = GetComponent<TextMeshPro>();
    }

    public void SetDamage(int damage)
    {
        // Устанавливаем текст в зависимости от типа компонента
    
        if (damageTextWorld != null)
        {
            damageTextWorld.text = damage.ToString();
            AnimateWorld();
        }
        else
        {
            Debug.LogError("No TextMeshPro component found on DamageNumber!");
        }
    }

    private void AnimateWorld()
    {
        // Анимация для мирового объекта
        Sequence sequence = DOTween.Sequence();

        // Подъем вверх
        sequence.Join(transform.DOMoveY(transform.position.y + riseHeight, riseDuration)
            .SetEase(Ease.OutQuad));

        // Исчезновение через изменение альфа канала цвета
        sequence.Join(damageTextWorld.DOFade(0f, riseDuration)
            .SetEase(fadeCurve));

        // Небольшое случайное смещение по X
        float randomX = Random.Range(-0.3f, 0.3f);
        sequence.Join(transform.DOMoveX(transform.position.x + randomX, riseDuration));

        // Масштабирование для эффекта
        sequence.Join(transform.DOScale(1.2f, riseDuration * 0.3f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo));

        // Уничтожение после анимации
        sequence.OnComplete(() => Destroy(gameObject));
    }
}
