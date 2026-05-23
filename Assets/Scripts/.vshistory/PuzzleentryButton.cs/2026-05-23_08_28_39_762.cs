using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PuzzleEntryButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Scene")]
    [Tooltip("Ќазвание сцены головоломки Ч должна быть добавлена в Build Settings")]
    public string puzzleSceneName = "PuzzleScene";

    [Header("Outline")]
    public Color outlineColor = new Color(0.2f, 0.5f, 1f, 1f);
    public float outlineWidth = 4f;

    [Header("Hover Animation")]
    public float hoverScale = 1.15f;
    public float animationSpeed = 8f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Outline outline;
    private bool isHovered = false;
    private bool isLoading = false;

    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        outline = GetComponent<Outline>();
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(outlineWidth, -outlineWidth);
        outline.enabled = true;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * animationSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLoading) return;
        isHovered = true;
        targetScale = originalScale * hoverScale;

        outline.effectColor = new Color(
            outlineColor.r,
            outlineColor.g,
            outlineColor.b,
            1f
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetScale = originalScale;
        outline.effectColor = outlineColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLoading) return;
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

    public void UnloadPuzzleScene()
    {
        Scene scene = SceneManager.GetSceneByName(puzzleSceneName);
        if (scene.isLoaded)
            SceneManager.UnloadSceneAsync(puzzleSceneName);
    }
}