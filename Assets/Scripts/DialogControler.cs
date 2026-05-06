using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogControler : MonoBehaviour
{
    public TextMeshProUGUI DialogText;
    public string[] Sentences;
    private int Index = 0;
    public float DialogSpeed;

    private bool isDialogueActive = false;
    private bool isTyping = false;

    void Start()
    {
        if (DialogText != null)
            DialogText.transform.parent.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isDialogueActive) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isTyping)
        {
            NextSentence();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseDialogue();
        }
    }

    public void StartDialogue(string[] newSentences)
    {
        Sentences = newSentences;
        Index = 0;
        isDialogueActive = true;

        if (DialogText != null)
            DialogText.transform.parent.gameObject.SetActive(true);

        DisablePlayerControls(true);

        NextSentence();
    }

    void CloseDialogue()
    {
        isDialogueActive = false;

        if (DialogText != null)
            DialogText.transform.parent.gameObject.SetActive(false);

        DisablePlayerControls(false);
    }

    void NextSentence()
    {
        if (Index <= Sentences.Length - 1)
        {
            DialogText.text = "";
            StartCoroutine(WriteSentence());
        }
        else
        {
            CloseDialogue();
        }
    }

    IEnumerator WriteSentence()
    {
        isTyping = true;

        foreach (char Character in Sentences[Index].ToCharArray())
        {
            DialogText.text += Character;
            yield return new WaitForSeconds(DialogSpeed);
        }

        Index++;
        isTyping = false;
    }

    void DisablePlayerControls(bool disable)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}