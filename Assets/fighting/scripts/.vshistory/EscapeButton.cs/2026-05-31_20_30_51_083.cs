using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeButton : MonoBehaviour
{
    public void OnExitClick()
    {
        string sceneName = FightSceneManager.CurrentFightScene;
        if (string.IsNullOrEmpty(sceneName)) return;

        Scene fightScene = SceneManager.GetSceneByName(sceneName);
        if (fightScene.isLoaded)
            SceneManager.UnloadSceneAsync(fightScene);
    }
}