using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneLoaderNewInput : MonoBehaviour
{
    [SerializeField] private string sceneName = "Level1";

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }
}