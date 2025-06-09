using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;

    private Vector2 movementInput;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetInput(Vector2 input)
    {
        movementInput = input;
    }

    private void FixedUpdate()
    {
        rb.velocity = movementInput * moveSpeed;
    }
}
