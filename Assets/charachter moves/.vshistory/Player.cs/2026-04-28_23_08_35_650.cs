using UnityEngine;

public class Player: MonoBehaviour
{
    private RigidBody2D rb;
    private PlayerInputActions playerInputActions;
    
    private void Awake()
    {
        rb = GetComponent<Rigitbody2D>();

        playerInputActions=new PlayerInputActions();
        playerInputActions.Enable();
    }

    private Vector2 GetMovementVector()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }
    private void Update()
    {
        Vector2 inputVector = GetMovementVector();
        rb.MovePosition(rb.position+inputVector);
    }

}
