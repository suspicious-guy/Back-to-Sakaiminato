using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogControler : MonoBehaviour
{
    [Header("UI элементы")]
    public GameObject dialogPanel;
    public Image playerPortrait;
    public Image npcPortrait;
    public TextMeshProUGUI dialogText;
    public TextMeshProUGUI speakerNameText;

    [Header("Настройки")]
    public float textSpeed = 0.05f;

    [Header("Блокировка игрока")]
    public Player playerController;  // ← добавить (перетащи игрока)

    public event Action OnDialogueEnd;

    private DialogueLine[] currentDialogue;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private bool isTyping = false;

    [Serializable]
    public class DialogueLine
    {
        public string speaker;
        public string speakerName;
        public string text;
    }

    void Start()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        if (playerPortrait != null)
            playerPortrait.gameObject.SetActive(false);

        if (npcPortrait != null)
            npcPortrait.gameObject.SetActive(false);

        if (speakerNameText != null)
            speakerNameText.gameObject.SetActive(false);

        // Автоматически ищем игрока, если не назначен
        if (playerController == null)
        {
            playerController = FindObjectOfType<Player>();
        }
    }

    void Update()
    {
        if (!isDialogueActive) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogText.text = currentDialogue[currentLineIndex].text;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(DialogueLine[] dialogue, Sprite npcSprite = null, bool showPlayerPortrait = false)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;

        // БЛОКИРУЕМ ДВИЖЕНИЕ ИГРОКА
        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
            Debug.Log("🔒 Движение игрока заблокировано");
        }

        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        if (npcSprite != null && npcPortrait != null)
            npcPortrait.sprite = npcSprite;

        if (showPlayerPortrait)
        {
            if (playerPortrait != null) playerPortrait.gameObject.SetActive(true);
            if (npcPortrait != null) npcPortrait.gameObject.SetActive(false);
        }
        else
        {
            if (playerPortrait != null) playerPortrait.gameObject.SetActive(false);
            if (npcPortrait != null) npcPortrait.gameObject.SetActive(true);
        }

        if (speakerNameText != null)
            speakerNameText.gameObject.SetActive(true);

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        if (currentLineIndex >= currentDialogue.Length)
        {
            CloseDialogue();
            return;
        }

        DialogueLine line = currentDialogue[currentLineIndex];

        if (speakerNameText != null && !string.IsNullOrEmpty(line.speakerName))
        {
            speakerNameText.text = line.speakerName;
        }

        if (line.speaker == "player")
        {
            Debug.Log($"Включаю PlayerPortrait, выключаю NPCPortrait");
            if (playerPortrait != null)
            {
                playerPortrait.gameObject.SetActive(true);
                Debug.Log($"PlayerPortrait активен: {playerPortrait.gameObject.activeSelf}");
            }
            if (npcPortrait != null)
                npcPortrait.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log($"Включаю NPCPortrait, выключаю PlayerPortrait");
            if (npcPortrait != null)
            {
                npcPortrait.gameObject.SetActive(true);
                Debug.Log($"NPCPortrait активен: {npcPortrait.gameObject.activeSelf}");
            }
            if (playerPortrait != null)
                playerPortrait.gameObject.SetActive(false);
        }

        dialogText.text = "";
        StartCoroutine(TypeText(line.text));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogText.text = "";

        foreach (char c in text.ToCharArray())
        {
            dialogText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        currentLineIndex++;
        ShowCurrentLine();
    }

    void CloseDialogue()
    {
        isDialogueActive = false;

        // РАЗБЛОКИРУЕМ ДВИЖЕНИЕ ИГРОКА
        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
            Debug.Log("🔓 Движение игрока разблокировано");
        }

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        if (playerPortrait != null)
            playerPortrait.gameObject.SetActive(false);

        if (npcPortrait != null)
            npcPortrait.gameObject.SetActive(false);

        if (speakerNameText != null)
            speakerNameText.gameObject.SetActive(false);

        Debug.Log("✅ Диалог закончился");
        OnDialogueEnd?.Invoke();
    }
}