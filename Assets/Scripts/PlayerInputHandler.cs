using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private PlayerAnimator animator;

    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.TriggerJump();
        }


        movement.SetInput(input);
        animator.UpdateAnimation(input);
    }
}
