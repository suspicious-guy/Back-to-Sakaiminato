using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    private Vector2 clickPosition;
    private bool hasClickTarget;

    // Выставляется из TreeCreature.Update (до Player.Update в том же кадре).
    // Сбрасывается в TryGetClickPosition — блокирует ровно один клик.
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

    // OnClick — InputSystem callback, вызывается ДО Update.
    // Здесь блок НЕ проверяем — флаг ещё не выставлен.
    // Просто сохраняем позицию клика.
    private void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        clickPosition = Camera.main.ScreenToWorldPoint(mousePos);
        hasClickTarget = true;
    }

    public Vector2 GetMovementVector()
        => playerInputActions.Player.Move.ReadValue<Vector2>();

    // TryGetClickPosition вызывается из Player.Update.
    // К этому моменту TreeCreature.Update уже выставил BlockNextClick,
    // поэтому проверяем блок здесь — это надёжно.
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
}