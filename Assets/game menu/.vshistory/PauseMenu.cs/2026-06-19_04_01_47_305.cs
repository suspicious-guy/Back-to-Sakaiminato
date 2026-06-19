using UnityEngine;
using UnityEngine.SceneManagement;


public class NewMonoBehaviourScript : MonoBehaviour
{
    public void ContinuePlaying()
    {
        string sceneName = "Pause Menu";
        if (string.IsNullOrEmpty(sceneName)) return;

        Scene currentScene = SceneManager.GetSceneByName(sceneName);
        if (currentScene.isLoaded)
            SceneManager.UnloadSceneAsync(fightScene);
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
