using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueGiver : MonoBehaviour
{
    public DialogControler.DialogueLine[] dialogue;
    public bool giveSecondSight = true;
    public bool oneTimeOnly = true;

    private bool used = false;
    private bool playerInRange = false;
    private DialogControler dialogController;
    private SecondSight secondSight;

    void Start()
    {
        dialogController = FindObjectOfType<DialogControler>();
        secondSight = FindObjectOfType<SecondSight>();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void Update()
    {
        if (playerInRange && !used && Keyboard.current.fKey.wasPressedThisFrame)
        {
            StartDialogue();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void StartDialogue()
    {
        if (dialogController != null && dialogue.Length > 0)
        {
            dialogController.OnDialogueEnd += GiveReward;
            dialogController.StartDialogue(dialogue);
            used = oneTimeOnly;
        }
    }

    void GiveReward()
    {
        if (giveSecondSight && secondSight != null)
        {
            secondSight.UnlockAbility();
            Debug.Log("✅ Способность разблокирована! Нажми T");
        }

        if (dialogController != null)
            dialogController.OnDialogueEnd -= GiveReward;
    }
}