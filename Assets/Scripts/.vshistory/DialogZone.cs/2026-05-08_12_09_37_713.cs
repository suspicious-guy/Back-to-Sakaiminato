using UnityEngine;

public class ZoneDialogueTrigger : MonoBehaviour
{
    [Header("Диалог")]
    public string[] sentences;           // Реплики (заполни в инспекторе)

    [Header("Настройки")]
    public bool oneTimeOnly = true;      // Запустить только один раз

    private bool used = false;
    private DialogControler dialogController;

    void Start()
    {
        // Находим диалоговый контроллер на сцене
        dialogController = FindObjectOfType<DialogControler>();

        if (dialogController == null)
            Debug.LogError("❌ DialogControler не найден! Добавь его на Canvas.");

        // Настраиваем коллайдер как триггер
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Если вошёл игрок и диалог ещё не использован
        if (other.CompareTag("Player") && !used)
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        if (dialogController == null)
        {
            Debug.LogError("DialogControler не найден!");
            return;
        }

        if (sentences == null || sentences.Length == 0)
        {
            Debug.LogError("Нет реплик! Заполни массив sentences в инспекторе.");
            return;
        }

        // Запускаем диалог из DialogControler
        dialogController.StartDialogue(sentences);

        // Если диалог только один раз, блокируем повторный запуск
        if (oneTimeOnly)
            used = true;
    }
}