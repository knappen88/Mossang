using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private Animator armsAnimator;

    [Header("Sprite renderers to flip")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer armsRenderer;

    private int lastDirection = 0;
    private bool isDirectionFrozen = false;
    private bool lastFacingRight = true; // Запоминаем последнее направление взгляда

    public void FreezeDirection() => isDirectionFrozen = true;
    public void UnfreezeDirection() => isDirectionFrozen = false;
    public bool IsDirectionFrozen => isDirectionFrozen;

    public void UpdateAnimation(Vector2 movementInput)
    {
        bool isWalking = movementInput.magnitude > 0.1f;

        int direction = lastDirection;

        if (!isDirectionFrozen && isWalking)
            direction = GetDirection(movementInput);

        bodyAnimator.SetBool("isWalking", isWalking);
        bodyAnimator.SetInteger("Direction", direction);

        armsAnimator.SetBool("isWalking", isWalking);
        armsAnimator.SetInteger("Direction", direction);

        HandleFlip(movementInput, direction);
        UpdateSortingOrder(direction);

        if (!isDirectionFrozen && isWalking)
            lastDirection = direction;
    }

    public void TriggerDeath()
    {
        Debug.Log("TriggerDeath() called");

        if (bodyAnimator != null)
            bodyAnimator.SetTrigger("Die");

        if (armsAnimator != null)
            armsAnimator.SetTrigger("Die");
    }

    public void TriggerJump()
    {
        Debug.Log("TriggerJump() called");

        if (bodyAnimator == null)
        {
            Debug.LogError("Body Animator is NULL!");
        }
        else
        {
            bodyAnimator.SetTrigger("Jump");
            Debug.Log("JumpBody");
        }

        if (armsAnimator == null)
        {
            Debug.LogError("Arms Animator is NULL!");
        }
        else
        {
            armsAnimator.SetTrigger("Jump");
            Debug.Log("JumpArms");
        }
    }

    private void UpdateSortingOrder(int direction)
    {
        if (direction == 1) // Back
        {
            armsRenderer.sortingOrder = bodyRenderer.sortingOrder + 1;
        }
        else
        {
            armsRenderer.sortingOrder = bodyRenderer.sortingOrder - 1;
        }
    }

    private int GetDirection(Vector2 input)
    {
        if (input == Vector2.zero)
            return lastDirection;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return 2; // Side (влево или вправо)
        }
        else
        {
            return input.y > 0 ? 1 : 0; // Back / Front
        }
    }

    private void HandleFlip(Vector2 input, int direction)
    {
        // Обновляем флип только для горизонтального движения
        if (Mathf.Abs(input.x) > 0.01f)
        {
            bool shouldFaceRight = input.x > 0;
            lastFacingRight = shouldFaceRight;

            bool flip = !shouldFaceRight;
            bodyRenderer.flipX = flip;
            armsRenderer.flipX = flip;
        }
        // При движении вверх/вниз восстанавливаем последнее направление взгляда
        else if (direction != 2 && Mathf.Abs(input.y) > 0.01f)
        {
            // Восстанавливаем флип на основе последнего направления
            bool flip = !lastFacingRight;
            bodyRenderer.flipX = flip;
            armsRenderer.flipX = flip;
        }
    }

    public void TriggerAttack()
    {
        if (bodyAnimator != null)
            bodyAnimator.SetTrigger("Attack");

        if (armsAnimator != null)
            armsAnimator.SetTrigger("Attack");
    }

    public int GetCurrentDirection()
    {
        return lastDirection;
    }

    public bool IsFacingRight()
    {
        return lastFacingRight;
    }
}