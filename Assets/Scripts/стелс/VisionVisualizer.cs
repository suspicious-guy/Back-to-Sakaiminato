using UnityEngine;

public class VisionVisualizer : MonoBehaviour
{
    public Color visionColor = new Color(1f, 0f, 0f, 0.3f);
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = visionColor;
            spriteRenderer.sortingOrder = -1;
        }
    }

    public void UpdateRadius(float radius)
    {
        if (spriteRenderer != null)
        {
            transform.localScale = Vector3.one * radius * 2f;
        }
    }
}