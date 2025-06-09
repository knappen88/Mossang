using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerJumpEffect : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpDistance = 1f;
    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private float jumpDuration = 0.4f;
    [SerializeField] private AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual Elements")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private GameObject shadowPrefab; // Префаб тени

    [Header("Effects")]
    [SerializeField] private float squashAmount = 0.85f;
    [SerializeField] private float stretchAmount = 1.2f;

    private PlayerMovement movement;
    private PlayerAnimator animator;
    private GameObject shadow;
    private Vector3 originalShadowScale;
    private bool isJumping;
    private float originalSortingOrder;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        animator = GetComponent<PlayerAnimator>();

        // Создаем тень
        if (shadowPrefab != null)
        {
            shadow = Instantiate(shadowPrefab, transform.position, Quaternion.identity);
            shadow.transform.SetParent(transform.parent);
            originalShadowScale = shadow.transform.localScale;
        }
    }

    private void Update()
    {
        
        if (shadow != null && !isJumping)
        {
            shadow.transform.position = new Vector3(transform.position.x, transform.position.y, shadow.transform.position.z);
        }
    }

    public void DoJump(Vector2 direction)
    {
        if (isJumping) return;

        isJumping = true;

        // Блокируем движение
        movement.DisableMovement();
        movement.SetVelocity(Vector2.zero);

        
        animator.TriggerJump();
        animator.FreezeDirection();

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos;

        
        if (direction != Vector2.zero)
        {
            targetPos += (Vector3)(direction.normalized * jumpDistance);
        }

        Sequence jumpSeq = DOTween.Sequence();

        
        jumpSeq.Append(visualTransform.DOScale(new Vector3(stretchAmount, squashAmount, 1f), jumpDuration * 0.2f)
            .SetEase(Ease.OutQuad));

        
        jumpSeq.AppendCallback(() => {
            
            if (direction != Vector2.zero)
            {
                transform.DOMove(targetPos, jumpDuration * 0.6f).SetEase(jumpCurve);
            }
        });

        
        jumpSeq.Append(visualTransform.DOScale(new Vector3(squashAmount, stretchAmount, 1f), jumpDuration * 0.3f)
            .SetEase(Ease.OutQuad));

        
        jumpSeq.Join(visualTransform.DOLocalMoveY(jumpHeight, jumpDuration * 0.3f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo));

        
        if (shadow != null)
        {
            
            if (direction != Vector2.zero)
            {
                jumpSeq.Join(shadow.transform.DOMove(new Vector3(targetPos.x, targetPos.y, shadow.transform.position.z), jumpDuration * 0.6f)
                    .SetEase(jumpCurve));
            }

            
            jumpSeq.Join(shadow.transform.DOScale(originalShadowScale * 0.6f, jumpDuration * 0.3f)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo));
        }

        
        jumpSeq.Append(visualTransform.DOScale(new Vector3(stretchAmount, squashAmount, 1f), jumpDuration * 0.1f)
            .SetEase(Ease.OutBounce));

        jumpSeq.Append(visualTransform.DOScale(Vector3.one, jumpDuration * 0.1f)
            .SetEase(Ease.OutQuad));

        jumpSeq.OnComplete(() =>
        {
            isJumping = false;
            visualTransform.localScale = Vector3.one;
            visualTransform.localPosition = Vector3.zero;
            animator.UnfreezeDirection();
            movement.EnableMovement();

            
            if (shadow != null)
            {
                shadow.transform.localScale = originalShadowScale;
            }
        });
    }

    public bool IsJumping => isJumping;

    private void OnDestroy()
    {
        if (shadow != null)
        {
            Destroy(shadow);
        }
    }
}