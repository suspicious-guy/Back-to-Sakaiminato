using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    private Vector2 clickPosition;
    private bool hasClickTarget;

    private static bool blockNextClick = false;
    public static void BlockNextClick()
    {
        blockNextClick = true;
        Debug.Log("[GameInput] —ледующий клик заблокирован дл€ движени€");
    }

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
            playerInputActions.Player.Click.performed -= OnClick;
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (blockNextClick)
        {
            blockNextClick = false;
            Debug.Log("[GameInput]  лик заблокирован Ч игрок не двигаетс€");
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        clickPosition = Camera.main.ScreenToWorldPoint(mousePos);
        hasClickTarget = true;
    }

    public Vector2 GetMovementVector()
        => playerInputActions.Player.Move.ReadValue<Vector2>();

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