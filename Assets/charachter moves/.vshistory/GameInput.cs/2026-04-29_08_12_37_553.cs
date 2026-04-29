using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    private PlayerInputActions playerInputActions;

    private Vector2 clickPosition;
    private bool hasClickTarget;
    private void Awake()
    {
        Instance = this;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
        playerInputActions.Player.Click.performed += OnClick;
    }
    private void OnDestroy()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Player.Click.performed -= OnClick;
        }
    }

    private void OnClick(InputAction.CallbackContext context)
    {

        Vector3 mousePos = Mouse.current.position.ReadValue();

        clickPosition = Camera.main.ScreenToWorldPoint(mousePos);
        clickPosition.z = 0;
        hasClickTarget = true;
    }
    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }
    public bool TryGetClickPosition(out Vector2 position)
    {
        if (hasClickTarget)
        {
            hasClickTarget = false;
            position = clickPosition;
            return true;
        }
        position = Vector2.zero;
        return false;
    }
}
