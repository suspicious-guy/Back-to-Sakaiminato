using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [Header("Обучающие вставки")]
    public GameObject[] tutorialScreens;  // Массив панелей с картинками

    [Header("Настройки")]
    public bool showOnce = true;  // Показывать только один раз

    private int currentIndex = 0;
    private bool isActive = false;
    private static bool hasBeenShown = false;  // Запоминает, показывали ли обучение
    private Player playerController;

    void Start()
    {
        playerController = FindObjectOfType<Player>();

        // Проверяем, нужно ли показывать обучение
        if (showOnce && hasBeenShown)
        {
            // Удаляем объекты обучения, если они не нужны
            foreach (var screen in tutorialScreens)
            {
                if (screen != null) Destroy(screen);
            }
            Destroy(gameObject);
            return;
        }

        StartTutorial();
    }

    void StartTutorial()
    {
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
            Time.timeScale = 0f;  // Останавливаем игру
        }
    }

    void Update()
    {
        if (!isActive) return;

        // Нажатие пробела для переключения
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            NextScreen();
        }
    }

    void NextScreen()
    {
        // Скрываем текущий экран
        if (tutorialScreens[currentIndex] != null)
            tutorialScreens[currentIndex].SetActive(false);

        currentIndex++;

        // Проверяем, есть ли следующий экран
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
        Time.timeScale = 1f;  // Возвращаем игру

        // Разблокируем игрока
        if (playerController != null)
            playerController.SetMovementEnabled(true);

        // Удаляем объекты обучения, чтобы не мешали
        foreach (var screen in tutorialScreens)
        {
            if (screen != null) Destroy(screen);
        }
        Destroy(gameObject);
    }
}