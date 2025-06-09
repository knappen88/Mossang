using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;

    private Vector2 movementInput;
    private Rigidbody2D rb;
    private bool canMove = true;
    private bool freezeDirection = false;

    public void FreezeDirection() => freezeDirection = true;
    public void UnfreezeDirection() => freezeDirection = false;
    public void DisableMovement() => canMove = false;
    public void EnableMovement() => canMove = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetInput(Vector2 input)
    {
        if (!freezeDirection)
            movementInput = input;
    }

    public void SetVelocity(Vector2 velocity)
    {
        rb.velocity = velocity;
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            rb.velocity = movementInput * moveSpeed;
        }
    }
}