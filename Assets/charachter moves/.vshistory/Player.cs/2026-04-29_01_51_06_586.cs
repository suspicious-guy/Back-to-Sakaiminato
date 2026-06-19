using UnityEngine;

public class Player: MonoBehaviour
{
    [SerializeField] private float movingSpeed = 5f;
    [SerializeField] private bool useClickMovement = true;

    private Vector2 targetPosition;
    private bool hasTarget;
    private Rigidbody2D rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (useClickMovement && hasTarget)
        {
            Vector2 direction = (targetPosition - rb.position).normalized;
            float distance = Vector2.Distance(rb.position, targetPosition);

            if (distance < 0.1f)
            {
                hasTarget = false;
                rb.velocity = Vector2.zero;
                return;
            }

            rb.MovePosition(rb.position + direction * (movingSpeed * Time.fixedDeltaTime));
        }
        else
        {
            Vector2 inputVector = GameInput.Instance.GetMovementVector();
            inputVector = inputVector.normalized;
            rb.MovePosition(rb.position + inputVector * (movingSpeed * Time.fixedDeltaTime));
        }
    }

}
