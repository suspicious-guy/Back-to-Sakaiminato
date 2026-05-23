using UnityEngine;

public class DialogueZoneTrigger : MonoBehaviour
{
    [Header("Диалог")]
    public DialogControler dialogController;
    public DialogControler.DialogueLine[] dialogueLines;

    [Header("Портрет игрока")]
    public Sprite playerPortrait;

    [Header("Настройки")]
    public bool oneTime = true;
    public bool autoFindController = true;

    private bool triggered = false;

    void Start()
    {
        if (autoFindController && dialogController == null)
        {
            dialogController = FindFirstObjectByType<DialogControler>();
            if (dialogController == null)
                Debug.LogError("❌ DialogControler не найден!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneTime && triggered) return;
        if (dialogueLines == null || dialogueLines.Length == 0) return;
        if (dialogController == null) return;

        triggered = true;

        dialogController.StartDialogue(dialogueLines, null, true);
        GetComponent<Collider2D>().enabled = false;
    }
}