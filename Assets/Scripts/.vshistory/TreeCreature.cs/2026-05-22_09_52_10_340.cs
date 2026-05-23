using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class CreatureStep
{
    [Tooltip("Позиция 'за деревом' для этого шага")]
    public Vector3 hiddenPosition;
    [Tooltip("Позиция 'выглянул' для этого шага")]
    public Vector3 visiblePosition;
    [Tooltip("Барьер, который снимется после клика (слой Obstacle, ObstacleRegistrar на нём)")]
    public GameObject barrier;
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[DefaultExecutionOrder(-10)]
public class TreeCreature : MonoBehaviour
{
    [Header("Шаги появления (по порядку)")]
    public CreatureStep[] steps;

    [Header("Таймер")]
    public float initialDelay = 2f;
    public float hideDelay = 5f;
    public float delayToNextStep = 2f;

    [Header("Подсветка")]
    public Color highlightColor = new Color(1f, 0.5f, 0.5f, 1f);
    public Color hoverColor = new Color(1f, 0.2f, 0.2f, 1f);
    public float pulseSpeed = 3f;
    public float pulseAmp = 0.2f;
    public float hoverScaleMultiplier = 1.2f;

    [Header("Скорость анимации")]
    public float moveSpeed = 8f;

    [Header("UI подсказка (опционально)")]
    public UnityEngine.UI.Text hintText;
    public string completionMessage = "Путь открыт!";

    private SpriteRenderer sr;
    private Collider2D col;
    private Vector3 baseScale;
    private Camera mainCam;

    private int currentStep = 0;
    private bool isVisible = false;
    private bool isHovered = false;
    private bool isClicked = false;
    private bool isFinished = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        baseScale = transform.localScale;
        mainCam = Camera.main;

        col.enabled = false;
        sr.color = Color.clear;

        UpdateHint();
        StartCoroutine(SequenceRoutine());
    }

    void Update()
    {
        if (isFinished) return;

        HandleHover();

        if (isVisible && !isClicked)
            HandleClickDetection();
    }

    void HandleHover()
    {
        if (!isVisible || isFinished) return;

        Vector2 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        bool over = col.OverlapPoint(mouseWorld);

        if (over && !isHovered)
        {
            isHovered = true;
            transform.localScale = baseScale * hoverScaleMultiplier;
        }
        else if (!over && isHovered)
        {
            isHovered = false;
            transform.localScale = baseScale;
        }

        float pulse = 1f + pulseAmp * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f);
        Color target = isHovered ? hoverColor : highlightColor;
        sr.color = new Color(
            Mathf.Clamp01(target.r * pulse),
            Mathf.Clamp01(target.g * pulse),
            Mathf.Clamp01(target.b * pulse),
            target.a
        );
    }

    void HandleClickDetection()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

        Debug.Log($"[TreeCreature] Клик в мире: {mouseWorld}, попадание: {(hit != null ? hit.gameObject.name : "null")}");

        if (hit != null && hit.gameObject == gameObject)
        {
            Debug.Log("[TreeCreature] Клик засчитан!");
            GameInput.BlockNextClick();

            isClicked = true;
            isHovered = false;
            transform.localScale = baseScale;
            col.enabled = false;
        }
        else
        {}
    }

    IEnumerator SequenceRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (currentStep < steps.Length && !isFinished)
        {
            CreatureStep step = steps[currentStep];

            transform.position = step.hiddenPosition;
            yield return StartCoroutine(AnimateTo(step.visiblePosition, true));

            Debug.Log($"[TreeCreature] Шаг {currentStep + 1}/{steps.Length} — жду клика ({hideDelay}с)");

            isClicked = false;
            float elapsed = 0f;

            while (!isClicked && elapsed < hideDelay)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (isClicked)
            {
                Debug.Log($"[TreeCreature] Клик на шаге {currentStep + 1}, снимаем барьер");
                RemoveBarrier(step);

                currentStep++;
                UpdateHint();

                if (currentStep >= steps.Length)
                {
                    isFinished = true;
                    Debug.Log("[TreeCreature] Все шаги пройдены!");
                    if (hintText != null) hintText.text = completionMessage;
                    StartCoroutine(HideHint(3f));
                    yield break;
                }

                yield return StartCoroutine(AnimateTo(step.hiddenPosition, false));
                yield return new WaitForSeconds(delayToNextStep);
            }
            else
            {
                Debug.Log($"[TreeCreature] Таймаут на шаге {currentStep + 1}, повтор");
                yield return StartCoroutine(AnimateTo(step.hiddenPosition, false));
                yield return new WaitForSeconds(delayToNextStep * 0.5f);
            }
        }
    }

    void RemoveBarrier(CreatureStep step)
    {
        if (step.barrier == null)
        {
            Debug.LogWarning("[TreeCreature] barrier == null для этого шага!");
            return;
        }

        Debug.Log($"[TreeCreature] Удаляем барьер: {step.barrier.name}");

        SpriteRenderer bsr = step.barrier.GetComponent<SpriteRenderer>();
        if (bsr != null)
            StartCoroutine(FadeBarrier(bsr, step.barrier));
        else
            Destroy(step.barrier);
    }

    IEnumerator FadeBarrier(SpriteRenderer bsr, GameObject barrierObj)
    {
        Collider2D barrierCol = barrierObj.GetComponent<Collider2D>();
        if (barrierCol != null)
        {
            barrierCol.enabled = false;
            Debug.Log($"[TreeCreature] Коллайдер барьера '{barrierObj.name}' отключён");
        }
        else
        {
            Debug.LogWarning($"[TreeCreature] На барьере '{barrierObj.name}' нет Collider2D!");
        }

        Color start = bsr.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            bsr.color = Color.Lerp(start, Color.clear, t);
            yield return null;
        }

        Debug.Log($"[TreeCreature] Destroy барьера '{barrierObj.name}'");
        Destroy(barrierObj);

        yield return null;

        if (PathfindingGrid.Instance != null)
        {
            PathfindingGrid.Instance.BuildGrid();
            Debug.Log("[TreeCreature] PathfindingGrid перестроена");
        }
        else
        {
            Debug.LogWarning("[TreeCreature] PathfindingGrid.Instance == null!");
        }
    }

    IEnumerator AnimateTo(Vector3 target, bool appearing)
    {
        isVisible = appearing;
        col.enabled = appearing;

        float t = 0f;
        Vector3 startPos = transform.position;
        Color startCol = sr.color;
        Color endCol = appearing ? highlightColor : Color.clear;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            transform.position = Vector3.Lerp(startPos, target, s);
            sr.color = Color.Lerp(startCol, endCol, s);
            yield return null;
        }

        transform.position = target;
        sr.color = endCol;
    }

    void UpdateHint()
    {
        if (hintText == null) return;
        if (currentStep < steps.Length)
            hintText.text = $"Найди существо ({currentStep + 1}/{steps.Length})";
    }

    IEnumerator HideHint(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hintText != null) hintText.text = "";
    }

    void OnDrawGizmosSelected()
    {
        if (steps == null) return;
        for (int i = 0; i < steps.Length; i++)
        {
            CreatureStep s = steps[i];
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(s.hiddenPosition, 0.15f);
            Gizmos.DrawLine(s.hiddenPosition, s.visiblePosition);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(s.visiblePosition, 0.25f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(s.visiblePosition + Vector3.up * 0.4f, $"Шаг {i + 1}");
#endif
            if (s.barrier != null)
            {
                Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.35f);
                Collider2D bc = s.barrier.GetComponent<Collider2D>();
                if (bc != null)
                    Gizmos.DrawCube(s.barrier.transform.position, bc.bounds.size);
            }
        }
    }
}