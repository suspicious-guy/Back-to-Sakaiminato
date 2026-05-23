using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeButton: MonoBehaviour
{
    public void OnExitClick()
    {
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}
