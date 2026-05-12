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

    [Header("Настройки")]
    public float textSpeed = 0.05f;

    public event Action OnDialogueEnd;

    private DialogueLine[] currentDialogue;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private bool isTyping = false;

    [Serializable]
    public class DialogueLine
    {
        public string speaker;
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

    public void StartDialogue(DialogueLine[] dialogue)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;

        if (dialogPanel != null)
            dialogPanel.SetActive(true);

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

        if (line.speaker == "player")
        {
            if (playerPortrait != null)
                playerPortrait.gameObject.SetActive(true);
            if (npcPortrait != null)
                npcPortrait.gameObject.SetActive(false);
        }
        else
        {
            if (playerPortrait != null)
                playerPortrait.gameObject.SetActive(false);
            if (npcPortrait != null)
                npcPortrait.gameObject.SetActive(true);
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

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        if (playerPortrait != null)
            playerPortrait.gameObject.SetActive(false);

        if (npcPortrait != null)
            npcPortrait.gameObject.SetActive(false);
        Debug.Log("✅ Диалог закончился");
        OnDialogueEnd?.Invoke();
    }
}