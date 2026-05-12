using UnityEngine;

public class DialogueZoneTrigger : MonoBehaviour
{
    [Header("Диалог")]
    [Tooltip("Перетащите сюда объект с DialogControler (обычно на UI Canvas)")]
    public DialogControler dialogController;

    [Tooltip("Массив реплик диалога")]
    public DialogControler.DialogueLine[] dialogueLines;

    [Header("Настройки")]
    public bool oneTime = true;  // Сработает только раз
    public bool autoFindController = true; // Автоматически найти контроллер

    private bool triggered = false;

    void Start()
    {
        // Автоматически ищем DialogControler на сцене, если не назначен вручную
        if (autoFindController && dialogController == null)
        {
            dialogController = FindFirstObjectByType<DialogControler>();
            if (dialogController == null)
            {
                Debug.LogError("❌ DialogControler не найден на сцене!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что в зону зашёл ИГРОК (по тегу или слою)
        if (!other.CompareTag("Player")) return;

        // Если диалог уже идёт — не запускаем новый
        // (нужно добавить публичное свойство в DialogControler, см. Шаг 2)
        // if (dialogController.IsDialogueActive) return;

        // Если зона одноразовая и уже сработала — выходим
        if (oneTime && triggered) return;

        // Если диалоговые строки не заданы — ошибка
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning($"⚠️ В зоне {gameObject.name} не заданы реплики диалога!");
            return;
        }

        // Запускаем диалог
        triggered = true;
        dialogController.StartDialogue(dialogueLines);

        // Если зона одноразовая — можно её отключить или удалить
        if (oneTime)
        {
            // Вариант 1: просто отключить коллайдер (остаётся в сцене)
            GetComponent<Collider2D>().enabled = false;

            // Вариант 2: отключить весь объект
            // gameObject.SetActive(false);

            // Вариант 3: удалить объект со сцены
            // Destroy(gameObject);
        }
    }
}