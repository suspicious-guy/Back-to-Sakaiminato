using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DialogueGiver : MonoBehaviour
{
    public DialogControler.DialogueLine[] dialogue;
    public bool giveSecondSight = true;
    public bool oneTimeOnly = true;
    public string fightSceneName = "TrialFighting";

    [Header("Обучение после диалога")]
    public bool startTutorialAfterDialogue = false;  // ← добавить
    public TutorialManager tutorialManager;          // ← добавить

    private bool used = false;
    private bool playerInRange = false;
    private DialogControler dialogController;
    private SecondSight secondSight;

    void Start()
    {
        dialogController = FindObjectOfType<DialogControler>();
        secondSight = FindObjectOfType<SecondSight>();

        // Автоматически ищем TutorialManager
        if (tutorialManager == null)
            tutorialManager = FindObjectOfType<TutorialManager>();

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
        else if (playerInRange && used && Keyboard.current.qKey.wasPressedThisFrame)
        {
            StartFight();
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
            dialogController.OnDialogueEnd += OnDialogueEnd;
            dialogController.StartDialogue(dialogue);
            used = oneTimeOnly;
        }
    }

    void OnDialogueEnd()  // ← переименовал GiveReward в OnDialogueEnd
    {
        if (giveSecondSight && secondSight != null)
        {
            secondSight.UnlockAbility();
            Debug.Log("✅ Способность разблокирована! Нажми T");
        }

        // ЗАПУСК ОБУЧЕНИЯ ПОСЛЕ ДИАЛОГА
        if (startTutorialAfterDialogue && tutorialManager != null)
        {
            tutorialManager.StartTutorial();
            Debug.Log("📖 Запуск обучения после диалога");
        }

        if (dialogController != null)
            dialogController.OnDialogueEnd -= OnDialogueEnd;
    }

    void StartFight()
    {
        FightSceneManager.CurrentFightScene = fightSceneName;
        SceneManager.LoadScene(fightSceneName, LoadSceneMode.Additive);
        Debug.Log($"[TreeCreature] Запуск файтинга: {fightSceneName}");
    }
}