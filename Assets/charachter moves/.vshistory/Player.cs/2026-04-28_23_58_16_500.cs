using UnityEngine;

public class Player: MonoBehaviour
{
    [SerializeField] private float movingSpeed = 5f;

    private Rigidbody2D rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 inputVector = GetMovementVector();
        rb.MovePosition(rb.position + inputVector * (movingSpeed * Time.fixedDeltaTime));

        inputVector=inputVector.normalized;
    }

}
