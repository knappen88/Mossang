using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private PlayerAnimator animator;
    [SerializeField] private PlayerJumpEffect jumpEffect;

    private PlayerMovement movement;
    private Vector2 lastInput;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        movement.SetInput(input);
        animator.UpdateAnimation(input);

        // Сохраняем последнее направление движения
        if (input != Vector2.zero)
        {
            lastInput = input;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !jumpEffect.IsJumping)
        {
            // Используем текущее направление если двигаемся, или последнее если стоим
            Vector2 jumpDirection = input != Vector2.zero ? input : lastInput;
            jumpEffect.DoJump(jumpDirection);
        }
    }
}