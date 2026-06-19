using UnityEngine;

public class PlayerPersistent : MonoBehaviour
{
    public static PlayerPersistent Instance;

    void Awake()
    {
        Debug.Log($"PlayerPersistent.Awake() на {gameObject.name}");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ Игрок сохранён между сценами, DontDestroyOnLoad применён");
        }
        else if (Instance != this)
        {
            Debug.Log("❌ Лишний игрок удалён");
            Destroy(gameObject);
        }
    }
}