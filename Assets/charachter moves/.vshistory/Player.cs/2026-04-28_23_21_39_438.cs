using UnityEngine;

public class Player: MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInputActions playerInputActions;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        playerInputActions=new PlayerInputActions();
        playerInputActions.Enable();
    }

    private Vector2 GetMovementVector()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }
    private void FixedUpdate()
    {
        Vector2 inputVector = GetMovementVector();
        rb.MovePosition(rb.position+inputVector);
    }

}
