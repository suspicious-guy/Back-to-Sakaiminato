using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PuzzleEntryButton : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Название сцены головоломки — должна быть добавлена в Build Settings")]
    public string puzzleSceneName = "PuzzleScene";

    [Header("Outline")]
    [Tooltip("Дочерний объект-дубликат спрайта чуть больше размером")]
    public SpriteRenderer outlineSprite;
    public Color outlineColor = new Color(0.2f, 0.5f, 1f, 1f);
    public Color completedOutlineColor = new Color(0.2f, 0.8f, 0.3f, 1f);

    [Header("Hover Animation")]
    public float hoverScale = 1.15f;
    public float animationSpeed = 8f;

    private PlayerInputActions input;
    private Collider2D col;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovered = false;
    private bool isLoading = false;
    private bool puzzleCompleted = false;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (outlineSprite != null)
            outlineSprite.color = outlineColor;

        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        input.Player.Enable();
        input.Player.Click.performed += OnClickPerformed;
    }

    void OnDisable()
    {
        input.Player.Click.performed -= OnClickPerformed;
        input.Player.Disable();
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * animationSpeed
        );

        UpdateHover();
    }

    void UpdateHover()
    {
        if (puzzleCompleted || isLoading) return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );
        bool over = col.OverlapPoint(mouseWorld);

        if (over && !isHovered)
        {
            isHovered = true;
            targetScale = originalScale * hoverScale;
        }
        else if (!over && isHovered)
        {
            isHovered = false;
            targetScale = originalScale;
        }
    }

    void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        if (!isHovered || isLoading || puzzleCompleted) return;
        StartCoroutine(LoadPuzzleAdditive());
    }

    private IEnumerator LoadPuzzleAdditive()
    {
        isLoading = true;
        targetScale = originalScale;

        Scene existing = SceneManager.GetSceneByName(puzzleSceneName);
        if (existing.isLoaded)
        {
            Debug.LogWarning($"Сцена {puzzleSceneName} уже загружена.");
            isLoading = false;
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(puzzleSceneName, LoadSceneMode.Additive);
        while (!op.isDone)
            yield return null;

        isLoading = false;
    }

    public void OnPuzzleCompleted()
    {
        puzzleCompleted = true;
        isHovered = false;
        targetScale = originalScale;

        if (outlineSprite != null)
            outlineSprite.color = completedOutlineColor;
    }

    public void UnloadPuzzleScene()
    {
        Scene scene = SceneManager.GetSceneByName(puzzleSceneName);
        if (scene.isLoaded)
            SceneManager.UnloadSceneAsync(puzzleSceneName);
    }
}