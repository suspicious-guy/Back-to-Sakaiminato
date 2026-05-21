// ============================================================
//  СУЩЕСТВО ИЗ-ЗА ДЕРЕВА — TreeCreature.cs
//  Прикрепи на объект существа.
//
//  Логика:
//  - Существо скрыто за деревом (позиция hiddenPos).
//  - По таймеру выглядывает (позиция visiblePos).
//  - Пока видно — блокирует проход через barricade-объект.
//  - Клик ЛКМ на существо — оно прячется, барьер снимается.
//
//  Настройка:
//  1. Создай спрайт существа, положи его за/у дерева.
//  2. Задай hiddenPosition (за деревом) и visiblePosition (выглянул).
//  3. Создай пустой GameObject "Barrier" с Collider2D (IsTrigger = false)
//     и назначь его в поле barrier.
//  4. Настрой appearInterval и hideDelay.
// ============================================================

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class TreeCreature : MonoBehaviour
{
    [Header("Позиции")]
    [Tooltip("Позиция, когда существо прячется за деревом")]
    public Vector3 hiddenPosition;
    [Tooltip("Позиция, когда существо выглядывает")]
    public Vector3 visiblePosition;

    [Header("Таймер")]
    [Tooltip("Через сколько секунд существо появляется после старта / после того как спрятали")]
    public float appearInterval = 4f;
    [Tooltip("Через сколько секунд существо само прячется, если игрок не нажал")]
    public float hideDelay = 3f;

    [Header("Барьер")]
    [Tooltip("Объект-стена, который блокирует проход (Collider2D, isTrigger = false)")]
    public GameObject barrier;

    [Header("Подсветка")]
    public Color normalColor = Color.white;
    public Color highlightColor = new Color(1f, 0.5f, 0.5f, 1f);
    public Color hoverColor = new Color(1f, 0.2f, 0.2f, 1f);
    [Tooltip("Пульсация при появлении")]
    public float pulseSpeed = 3f;
    public float pulseAmp = 0.2f;
    public float hoverScaleMultiplier = 1.2f;

    [Header("Скорость анимации")]
    public float moveSpeed = 8f;

    // --- внутреннее ---
    private SpriteRenderer sr;
    private Collider2D col;
    private bool isVisible = false;
    private bool isHovered = false;
    private bool isDefeated = false;   // существо побеждено навсегда
    private Vector3 baseScale;
    private Coroutine hideCoroutine;

    // -------------------------------------------------------
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        baseScale = transform.localScale;

        // Стартуем в спрятанном состоянии
        transform.position = hiddenPosition;
        col.enabled = false;
        sr.color = Color.clear;

        // Барьер активен по умолчанию
        SetBarrier(true);

        StartCoroutine(AppearLoop());
    }

    // -------------------------------------------------------
    void Update()
    {
        if (!isVisible || isDefeated) return;

        // Пульсация
        float pulse = 1f + pulseAmp * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f);
        Color target = isHovered ? hoverColor : highlightColor;
        sr.color = new Color(target.r * pulse, target.g * pulse, target.b * pulse, target.a);
    }

    // -------------------------------------------------------
    IEnumerator AppearLoop()
    {
        while (!isDefeated)
        {
            // Ждём перед появлением
            yield return new WaitForSeconds(appearInterval);

            if (isDefeated) yield break;

            // Появляемся
            yield return StartCoroutine(MoveTo(visiblePosition, true));

            // Ждём, пока игрок нажмёт или время выйдет
            hideCoroutine = StartCoroutine(AutoHide());
            yield return hideCoroutine;
            hideCoroutine = null;
        }
    }

    // -------------------------------------------------------
    IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(hideDelay);
        if (isVisible)
            yield return StartCoroutine(MoveTo(hiddenPosition, false));
    }

    // -------------------------------------------------------
    IEnumerator MoveTo(Vector3 target, bool appearing)
    {
        isVisible = appearing;
        col.enabled = appearing;

        if (appearing)
        {
            // Моментально ставим позицию старта и проявляем
            transform.position = hiddenPosition;
            sr.color = Color.clear;
        }

        float t = 0f;
        Vector3 start = transform.position;
        Color startColor = sr.color;
        Color endColor = appearing ? highlightColor : Color.clear;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            sr.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        transform.position = target;
        sr.color = endColor;
    }

    // -------------------------------------------------------
    void OnMouseEnter()
    {
        if (!isVisible || isDefeated) return;
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
        if (!isVisible || isDefeated) return;
        Dismiss();
    }

    // -------------------------------------------------------
    /// <summary>Игрок кликнул на существо — прячем навсегда, снимаем барьер</summary>
    void Dismiss()
    {
        isDefeated = true;
        transform.localScale = baseScale;

        // Останавливаем таймер автоскрытия
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        // Снимаем барьер — игрок может пройти
        SetBarrier(false);

        Debug.Log("[TreeCreature] Существо прогнано! Барьер снят.");

        // Анимация исчезновения
        StartCoroutine(DismissAnimation());
    }

    IEnumerator DismissAnimation()
    {
        col.enabled = false;
        float t = 0f;
        Color startColor = sr.color;
        Vector3 startScale = transform.localScale;

        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float p = t / 0.5f;
            sr.color = Color.Lerp(startColor, Color.clear, p);
            transform.localScale = startScale * (1f + p * 0.4f);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    void SetBarrier(bool active)
    {
        if (barrier != null)
            barrier.SetActive(active);
    }

    // -------------------------------------------------------
    // Гизмо для удобной настройки позиций в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(hiddenPosition, 0.2f);
        Gizmos.DrawLine(hiddenPosition, visiblePosition);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(visiblePosition, 0.3f);

        if (barrier != null)
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
            Collider2D bc = barrier.GetComponent<Collider2D>();
            if (bc != null)
                Gizmos.DrawCube(barrier.transform.position, bc.bounds.size);
        }
    }
}