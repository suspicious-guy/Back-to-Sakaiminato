using UnityEngine;

public class DialogZone : MonoBehaviour
{
    public string[] sentences;
    public bool oneTimeOnly = true;

    private bool used = false;
    private DialogControler dialogController;

    void Start()
    {
        dialogController = FindObjectOfType<DialogControler>();

        if (dialogController == null)
            Debug.LogError("❌ ZoneDialogueTrigger: DialogControler НЕ НАЙДЕН на сцене!");
        else
            Debug.Log("✅ ZoneDialogueTrigger: DialogControler найден");

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
            Debug.LogError("❌ ZoneDialogueTrigger: Нет Collider2D!");
        else if (!col.isTrigger)
            Debug.LogError("❌ ZoneDialogueTrigger: Collider2D не является триггером (Is Trigger = false)!");
        else
            Debug.Log("✅ ZoneDialogueTrigger: Коллайдер настроен правильно");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"❗ ZoneDialogueTrigger: Вход в триггер. Объект: {other.name}, Тег: {other.tag}");

        if (other.CompareTag("Player"))
        {
            Debug.Log("✅ ZoneDialogueTrigger: Игрок вошёл в зону!");

            if (used)
            {
                Debug.Log("⚠️ ZoneDialogueTrigger: Диалог уже использован (oneTimeOnly = true)");
                return;
            }

            if (sentences == null || sentences.Length == 0)
            {
                Debug.LogError("❌ ZoneDialogueTrigger: Массив sentences пустой! Заполни в инспекторе.");
                return;
            }

            Debug.Log($"💬 ZoneDialogueTrigger: Запускаю диалог с {sentences.Length} строками");
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        if (dialogController != null)
        {
            dialogController.StartDialogue(sentences);
            used = oneTimeOnly;
            Debug.Log("✅ ZoneDialogueTrigger: Диалог запущен через dialogController.StartDialogue()");
        }
        else
        {
            Debug.LogError("❌ ZoneDialogueTrigger: dialogController = null, диалог не запущен!");
        }
    }

    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
}