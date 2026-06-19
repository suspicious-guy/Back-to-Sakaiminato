using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public void OnExitClick()
    {
        string sceneName = "Pause Menu";
        if (string.IsNullOrEmpty(sceneName)) return;

        Scene currentScene = SceneManager.GetSceneByName(sceneName);
        if (currentScene.isLoaded)
            SceneManager.UnloadSceneAsync(fightScene);
    }
}
