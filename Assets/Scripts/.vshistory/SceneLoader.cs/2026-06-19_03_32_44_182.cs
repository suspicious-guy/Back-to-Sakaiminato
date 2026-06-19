using UnityEngine;
using UnityEngine.InputSystem; // Библиотека новой системы ввода
using UnityEngine.SceneManagement;

public class SceneLoaderNewInput : MonoBehaviour
{
    [SerializeField] private string sceneName = "Level1";

    void Update()
    {
        // Пример для клавиши Space
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}