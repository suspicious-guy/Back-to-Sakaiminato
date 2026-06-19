using UnityEngine;
using UnityEngine.SceneManagement;


public class PauseMenu : MonoBehaviour
{
    public void ContinuePlaying()
    {
        string sceneName = "Pause Menu";
        if (string.IsNullOrEmpty(sceneName)) return;

        Scene currentScene = SceneManager.GetSceneByName(sceneName);
        if (currentScene.isLoaded)
            SceneManager.UnloadSceneAsync(currentScene);
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
    }
}
