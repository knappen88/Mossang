using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private Animator armsAnimator;

    [Header("Sprite renderers to flip")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer armsRenderer;

    private int lastDirection = 0;


    public void UpdateAnimation(Vector2 movementInput)
    {
        bool isWalking = movementInput.magnitude > 0.1f;
        int direction = isWalking ? GetDirection(movementInput) : lastDirection;

        bodyAnimator.SetBool("isWalking", isWalking);
        bodyAnimator.SetInteger("Direction", direction);

        armsAnimator.SetBool("isWalking", isWalking);
        armsAnimator.SetInteger("Direction", direction);

        HandleFlip(movementInput);
        UpdateSortingOrder(direction);

        if (isWalking)
            lastDirection = direction;
    }

    public void TriggerJump()
    {
        bodyAnimator.SetTrigger("Jump");
        armsAnimator.SetTrigger("Jump");
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
            return 0;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return 2; // Side (влево или вправо)
        }
        else
        {
            return input.y > 0 ? 1 : 0; // Back / Front
        }
    }

    private void HandleFlip(Vector2 input)
    {
        if (Mathf.Abs(input.x) > 0.01f)
        {
            bool flip = input.x < 0;
            bodyRenderer.flipX = flip;
            armsRenderer.flipX = flip;
        }
    }
}
