using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;
    private Vector2 clickPosition;
    private bool hasClickTarget;
    private static bool blockNextClick = false;

    public static void BlockNextClick() => blockNextClick = true;

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
        Vector2 mousePos = Mouse.current.position.ReadValue();
        clickPosition = Camera.main.ScreenToWorldPoint(mousePos);
        hasClickTarget = true;
        Debug.Log($"[GameInput] Клик зафиксирован: {clickPosition}");
    }

    public Vector2 GetMovementVector()
        => playerInputActions.Player.Move.ReadValue<Vector2>();

    public bool PeekClickPosition(out Vector2 position)
    {
        if (hasClickTarget)
        {
            position = clickPosition;
            return true;
        }
        position = Vector2.zero;
        return false;
    }

    public void ConsumeClick()
    {
        hasClickTarget = false;
        blockNextClick = false;
    }

    public bool TryGetClickPosition(out Vector2 position)
    {
        if (hasClickTarget)
        {
            hasClickTarget = false;
            if (blockNextClick)
            {
                blockNextClick = false;
                Debug.Log("[GameInput] Клик заблокирован — игрок не двигается");
                position = Vector2.zero;
                return false;
            }
            position = clickPosition;
            return true;
        }
        position = Vector2.zero;
        return false;
    }

    private void OnDisable()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Player.Disable();
            playerInputActions.Disable();
        }
    }
}