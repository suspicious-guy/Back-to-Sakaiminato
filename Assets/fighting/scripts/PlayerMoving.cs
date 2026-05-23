using UnityEngine;
using UnityEngine.UI;

public class PlayerMoving: MonoBehaviour
{
    public float speed = 300f;

    private RectTransform rectTransform;
    private float halfWidth;
    private float halfHeight;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        halfWidth = rectTransform.rect.width / 2;
        halfHeight = rectTransform.rect.height / 2;
    }

    void Update()
    {
        Vector2 moveInput = GameInput.Instance.GetMovementVector();
        if (moveInput == Vector2.zero) return;

        Vector3 newPosition = rectTransform.localPosition;
        newPosition.x += moveInput.x * speed * Time.deltaTime;
        newPosition.y += moveInput.y * speed * Time.deltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x,
            BattleFieldBoundary.MinX + halfWidth,
            BattleFieldBoundary.MaxX - halfWidth);

        newPosition.y = Mathf.Clamp(newPosition.y,
            BattleFieldBoundary.MinY + halfHeight,
            BattleFieldBoundary.MaxY - halfHeight);

        rectTransform.localPosition = newPosition;
    }
}