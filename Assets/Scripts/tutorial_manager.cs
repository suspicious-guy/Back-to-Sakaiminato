using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [Header("Обучающие вставки")]
    public GameObject[] tutorialScreens;

    [Header("Настройки")]
    public bool showOnce = true;

    private int currentIndex = 0;
    private bool isActive = false;
    private static bool hasBeenShown = false;
    private Player playerController;

    void Start()
    {
        playerController = FindObjectOfType<Player>();

        // НЕ ЗАПУСКАЕМ АВТОМАТИЧЕСКИ!
        // Обучение будет запущено по вызову из диалога
    }

    // Публичный метод для запуска обучения из диалога
    public void StartTutorial()
    {
        if (showOnce && hasBeenShown) return;

        // Блокируем игрока
        if (playerController != null)
            playerController.SetMovementEnabled(false);

        // Скрываем все экраны
        foreach (var screen in tutorialScreens)
        {
            if (screen != null)
                screen.SetActive(false);
        }

        // Показываем первый экран
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
        hasBeenShown = true;
        Time.timeScale = 1f;

        if (playerController != null)
            playerController.SetMovementEnabled(true);

        // Удаляем объекты обучения
        foreach (var screen in tutorialScreens)
        {
            if (screen != null) Destroy(screen);
        }
        Destroy(gameObject);
    }
}