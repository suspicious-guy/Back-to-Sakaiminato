using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStealth : MonoBehaviour
{
    [Header("Настройки укрытия")]
    public LayerMask grassLayer;
    public KeyCode crouchKey = KeyCode.Z;

    [Header("Визуализация")]
    public GameObject crouchIndicator;  // Иконка/текст "Присел"

    private bool isCrouching = false;
    private bool isInGrass = false;
    private float originalSpeed;
    private Player playerMovement;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public bool isHidden => isCrouching && isInGrass;

    void Start()
    {
        playerMovement = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (crouchIndicator != null)
            crouchIndicator.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.zKey.wasPressedThisFrame || Keyboard.current.lKey.wasPressedThisFrame)
        {
            ToggleCrouch();
        }

        CheckIfInGrass();
        UpdateVisuals();
    }

    void ToggleCrouch()
    {
        isCrouching = !isCrouching;

        //if (playerMovement != null)
        //{
        //    if (isCrouching)
        //        playerMovement.SetSpeedMultiplier(0.5f);
        //    else
        //        playerMovement.SetSpeedMultiplier(1f);
        //}

        Debug.Log(isCrouching ? "🔻 Игрок присел" : "🔺 Игрок встал");
    }

    void CheckIfInGrass()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.3f, grassLayer);
        isInGrass = hit != null;
    }

    void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            if (isHidden)
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            else
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

        if (crouchIndicator != null)
        {
            crouchIndicator.SetActive(isCrouching);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isHidden ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}