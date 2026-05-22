using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Настройки точки появления")]
    public string pointId = "entrance";
    public string targetLayer = "Default";  // Слой для персонажа

    void Start()
    {
        if (PlayerPrefs.GetString("SpawnPoint") == pointId)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = transform.position;

                // Устанавливаем слой
                player.layer = LayerMask.NameToLayer(targetLayer);
                Debug.Log($"✅ Игрок появился в {pointId} на слое {targetLayer}");
            }
            else
            {
                Debug.LogError("❌ Игрок не найден! Проверь тег 'Player'");
            }

            PlayerPrefs.DeleteKey("SpawnPoint");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}