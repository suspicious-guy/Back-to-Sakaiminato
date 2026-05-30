using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistent : MonoBehaviour
{
    public static PlayerPersistent Instance;

    [Tooltip("Сцены в которых игрок скрыт и не двигается")]
    public string[] hiddenInScenes;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldHide = System.Array.IndexOf(hiddenInScenes, scene.name) >= 0;
        SetPlayerActive(!shouldHide);
    }

    void SetPlayerActive(bool active)
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = active;
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = active;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = active;
        }

        var player = GetComponent<Player>();
        if (player != null)
            player.enabled = active;
    }
}