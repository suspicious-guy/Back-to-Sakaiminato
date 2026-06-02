using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeButton : MonoBehaviour
{
    public void OnExitClick()
    {
        Scene fightScene = SceneManager.GetSceneByName("FightingShirime");
        if (fightScene.isLoaded)
            SceneManager.UnloadSceneAsync(fightScene);
    }
}