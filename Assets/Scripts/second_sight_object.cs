using UnityEngine;

public class RevealableObject : MonoBehaviour
{
    public bool startVisible = false;
    public bool autoHide = true;
    public Color revealColor = Color.cyan;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isVisible;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            isVisible = startVisible;

            if (!startVisible)
            {
                SetAlpha(0f);
            }
        }
    }

    void SetAlpha(float alpha)
    {
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
    public void OnSecondSightActivate()
    {
        Debug.Log($"Activate: {gameObject.name}");
        if (spriteRenderer == null) return;

        SetAlpha(1f);
        spriteRenderer.color = new Color(revealColor.r, revealColor.g, revealColor.b, 1f);
        Debug.Log($"Alpha после Activate: {spriteRenderer.color.a}");
    }

    public void OnSecondSightDeactivate()
    {
        Debug.Log($"Deactivate: {gameObject.name}");
        if (spriteRenderer == null) return;

        if (!startVisible && autoHide)
        {
            SetAlpha(0f);
            Debug.Log($"Alpha после Deactivate: {spriteRenderer.color.a}");
        }
    }
}