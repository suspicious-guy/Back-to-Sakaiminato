using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Collections.Unicode;

public class Player: MonoBehaviour
{
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 8f;

    private float currentSpeed;
    private bool isRunning = false;
    private Vector2 targetPosition;
    private bool hasTarget;
    private Rigidbody2D rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            isRunning = !isRunning;
            currentSpeed = isRunning ? runSpeed : walkSpeed;
            Debug.Log($"Текущая скорость: {currentSpeed}, бег: {isRunning}");
        }

        if (GameInput.Instance.TryGetClickPosition(out Vector2 clickPos))
        {
            targetPosition = clickPos;
            hasTarget = true;
        }
    }
    private void FixedUpdate()
    {
        float speedToUse = currentSpeed;
        if (hasTarget)
        {
            Vector2 direction = (targetPosition - rb.position).normalized;
            float distance = Vector2.Distance(rb.position, targetPosition);

            if (distance < 0.1f)
            {
                hasTarget = false;
                rb.linearVelocity = Vector2.zero;
                return;
            }

            rb.MovePosition(rb.position + direction * (speedToUse * Time.fixedDeltaTime));
        }
        else
        {
            Vector2 inputVector = GameInput.Instance.GetMovementVector();
            inputVector = inputVector.normalized;

            if (inputVector != Vector2.zero)
            {
                Debug.Log($"Движение WASD, скорость: {speedToUse}");
            }

            rb.MovePosition(rb.position + inputVector * (speedToUse * Time.fixedDeltaTime));
        }
    }

}
