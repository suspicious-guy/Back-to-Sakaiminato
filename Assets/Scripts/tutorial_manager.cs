using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [Header("Обучающие вставки")]
    public GameObject[] tutorialScreens;

    [Header("Настройки")]
    public bool showOnce = true;
    public bool startOnGameStart = false;  // ← запускать в начале игры?

    private int currentIndex = 0;
    private bool isActive = false;
    private static bool hasBeenShown = false;  // для обучения в начале
    private static bool hasBeenShownSecond = false;  // для обучения после диалога
    private Player playerController;

    void Start()
    {
        playerController = FindObjectOfType<Player>();

        // Запускаем обучение в начале игры
        if (startOnGameStart && !hasBeenShown)
        {
            StartTutorial();
        }
    }

    public void StartTutorial()
    {
        // Проверяем, какое обучение запускаем
        if (startOnGameStart && hasBeenShown) return;
        if (!startOnGameStart && hasBeenShownSecond) return;

        Debug.Log($"📖 Запуск обучения: {(startOnGameStart ? "начало игры" : "после диалога")}");

        if (playerController != null)
            playerController.SetMovementEnabled(false);

        foreach (var screen in tutorialScreens)
        {
            if (screen != null)
                screen.SetActive(false);
        }

        if (tutorialScreens.Length > 0 && tutorialScreens[0] != null)
        {
            tutorialScreens[0].SetActive(true);
            isActive = true;
            currentIndex = 0;
            Time.timeScale = 0f;
        }
    }

    void Update()
    {
        if (!isActive) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            NextScreen();
        }
    }

    void NextScreen()
    {
        if (tutorialScreens[currentIndex] != null)
            tutorialScreens[currentIndex].SetActive(false);

        currentIndex++;

        if (currentIndex < tutorialScreens.Length)
        {
            if (tutorialScreens[currentIndex] != null)
                tutorialScreens[currentIndex].SetActive(true);
        }
        else
        {
            EndTutorial();
        }
    }

    void EndTutorial()
    {
        isActive = false;
        Time.timeScale = 1f;

        if (playerController != null)
            playerController.SetMovementEnabled(true);

        // Запоминаем, какое обучение прошли
        if (startOnGameStart)
            hasBeenShown = true;
        else
            hasBeenShownSecond = true;

        foreach (var screen in tutorialScreens)
        {
            if (screen != null) Destroy(screen);
        }
        Destroy(gameObject);
    }
}