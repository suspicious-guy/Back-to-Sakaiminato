using System.Collections;
using UnityEngine;

[System.Serializable]
public class CreatureStep
{
    [Tooltip("Позиция 'за деревом' для этого шага")]
    public Vector3 hiddenPosition;
    [Tooltip("Позиция 'выглянул' для этого шага")]
    public Vector3 visiblePosition;
    [Tooltip("Барьер, который снимется после клика на этом шаге (слой Obstacle)")]
    public GameObject barrier;
}


[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class TreeCreature : MonoBehaviour
{
    [Header("Шаги появления (по порядку)")]
    public CreatureStep[] steps;

    [Header("Таймер")]
    [Tooltip("Задержка перед первым появлением (сек)")]
    public float initialDelay = 2f;
    [Tooltip("Через сколько секунд существо само прячется, если не кликнули")]
    public float hideDelay = 5f;
    [Tooltip("Пауза после клика перед появлением в следующей точке (сек)")]
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

        col.enabled = false;
        sr.color = Color.clear;

        UpdateHint();
        StartCoroutine(SequenceRoutine());
    }

    void Update()
    {
        if (!isVisible || isFinished) return;

        float pulse = 1f + pulseAmp * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f);
        Color target = isHovered ? hoverColor : highlightColor;
        sr.color = new Color(
            Mathf.Clamp01(target.r * pulse),
            Mathf.Clamp01(target.g * pulse),
            Mathf.Clamp01(target.b * pulse),
            target.a
        );
    }

    IEnumerator SequenceRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (currentStep < steps.Length && !isFinished)
        {
            CreatureStep step = steps[currentStep];

            transform.position = step.hiddenPosition;

            yield return StartCoroutine(AnimateTo(step.visiblePosition, true));

            isClicked = false;
            float elapsed = 0f;

            while (!isClicked && elapsed < hideDelay)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (isClicked)
            {
                RemoveBarrier(step);

                currentStep++;
                UpdateHint();

                if (currentStep >= steps.Length)
                {
                    isFinished = true;
                    Debug.Log("[TreeCreature] Все барьеры сняты!");
                    if (hintText != null) hintText.text = completionMessage;
                    StartCoroutine(HideHint(3f));
                    yield break;
                }

                yield return StartCoroutine(AnimateTo(step.hiddenPosition, false));

                yield return new WaitForSeconds(delayToNextStep);
            }
            else
            {
                yield return StartCoroutine(AnimateTo(step.hiddenPosition, false));
                yield return new WaitForSeconds(delayToNextStep * 0.5f);
            }
        }
    }

    void OnMouseEnter()
    {
        if (!isVisible || isFinished) return;
        isHovered = true;
        transform.localScale = baseScale * hoverScaleMultiplier;
    }

    void OnMouseExit()
    {
        isHovered = false;
        transform.localScale = baseScale;
    }

    void OnMouseDown()
    {
        if (!isVisible || isFinished || isClicked) return;

        CreatureClickBlocker.IsCreatureClicked = true;

        isClicked = true;
        isHovered = false;
        transform.localScale = baseScale;
        col.enabled = false;
    }

    void RemoveBarrier(CreatureStep step)
    {
        if (step.barrier == null) return;

        Collider2D barrierCol = step.barrier.GetComponent<Collider2D>();
        if (barrierCol != null) barrierCol.enabled = false;

        SpriteRenderer bsr = step.barrier.GetComponent<SpriteRenderer>();
        if (bsr != null)
            StartCoroutine(FadeBarrier(bsr, step.barrier));
        else
            step.barrier.SetActive(false);
    }

    IEnumerator FadeBarrier(SpriteRenderer bsr, GameObject barrierObj)
    {
        Color start = bsr.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            bsr.color = Color.Lerp(start, Color.clear, t);
            yield return null;
        }
        barrierObj.SetActive(false);
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
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f + 0.5f * i / Mathf.Max(steps.Length - 1, 1));
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