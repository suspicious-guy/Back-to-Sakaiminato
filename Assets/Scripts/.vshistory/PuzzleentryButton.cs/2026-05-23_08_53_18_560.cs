using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PuzzleEntryButton : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Ќазвание сцены головоломки Ч должна быть добавлена в Build Settings")]
    public string puzzleSceneName = "PuzzleScene";

    [Header("Outline")]
    [Tooltip("ќбъект-дубликат спрайта чуть больше размером Ч создай его вручную (см. инструкцию)")]
    public SpriteRenderer outlineSprite;
    public Color outlineColor = new Color(0.2f, 0.5f, 1f, 1f);
    public Color completedOutlineColor = new Color(0.2f, 0.8f, 0.3f, 1f);

    [Header("Hover Animation")]
    public float hoverScale = 1.15f;
    public float animationSpeed = 8f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private SpriteRenderer sr;
    private bool isHovered = false;
    private bool isLoading = false;
    private bool puzzleCompleted = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (outlineSprite != null)
            outlineSprite.color = outlineColor;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * animationSpeed
        );

        HandleHover();
        HandleClick();
    }

    void HandleHover()
    {
        if (puzzleCompleted || isLoading) return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool overSprite = GetComponent<Collider2D>().OverlapPoint(mouseWorld);

        if (overSprite && !isHovered)
        {
            isHovered = true;
            targetScale = originalScale * hoverScale;
        }
        else if (!overSprite && isHovered)
        {
            isHovered = false;
            targetScale = originalScale;
        }
    }

    void HandleClick()
    {
        if (!isHovered || isLoading || puzzleCompleted) return;

        if (Input.GetMouseButtonDown(0))
            StartCoroutine(LoadPuzzleAdditive());
    }

    private IEnumerator LoadPuzzleAdditive()
    {
        isLoading = true;
        targetScale = originalScale;

        Scene existing = SceneManager.GetSceneByName(puzzleSceneName);
        if (existing.isLoaded)
        {
            Debug.LogWarning($"—цена {puzzleSceneName} уже загружена.");
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
        targetScale = originalScale;
        isHovered = false;

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