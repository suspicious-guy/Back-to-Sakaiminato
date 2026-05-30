using System.Collections;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Настройки точки появления")]
    public string pointId = "entrance";
    public string targetLayer = "Player";
    public int orderInLayer = 0;

    void Start()
    {
        StartCoroutine(TeleportPlayer());
    }

    IEnumerator TeleportPlayer()
    {
        yield return null;

        string saved = PlayerPrefs.GetString("SpawnPoint");
        Debug.Log($"🔍 SpawnPoint: saved='{saved}', pointId='{pointId}', match={saved == pointId}");

        if (saved == pointId)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log($"🔍 Player found: {player != null}");

            if (player != null)
            {
                player.transform.position = transform.position;
                // Перемещаем игрока
                player.transform.position = transform.position;

                // Настройка слоя
                int layerIndex = LayerMask.NameToLayer(targetLayer);
                if (layerIndex == -1) layerIndex = 0;
                player.layer = layerIndex;

                // Настройка сортировки спрайта
                SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = orderInLayer;

                // ===== ОБНОВЛЯЕМ КАМЕРУ =====
                Camera cam = Camera.main;
                if (cam != null)
                {
                    TopDownCamera3_4_ cameraScript = cam.GetComponent<TopDownCamera3_4_>();
                    if (cameraScript != null)
                    {
                        cameraScript.SetPlayer(player.transform);
                        Debug.Log("✅ Камера обновлена");
                    }
                }
                // ============================

                // Обновляем IsometricDepthSorter
                IsometricDepthSorter[] sorters = FindObjectsOfType<IsometricDepthSorter>();
                foreach (var sorter in sorters)
                {
                    if (sorter != null)
                        sorter.SetPlayer(player);
                }

                Debug.Log($"✅ Игрок появился в {pointId}");
            }
            else
            {
                Debug.LogError("❌ Игрок не найден!");
            }

            PlayerPrefs.DeleteKey("SpawnPoint");
        }
    }
}