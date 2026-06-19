using System.Collections;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Настройки точки появления")]
    public string pointId = "entrance";
    public string targetLayer = "Player";
    public int orderInLayer = 0;

    [Header("Использовать как стартовую точку")]
    [Tooltip("Если true - игрок появится здесь при загрузке сцены")]
    public bool isDefaultSpawn = false;

    void Start()
    {
        StartCoroutine(TeleportPlayer());
    }

    IEnumerator TeleportPlayer()
    {
        yield return null;

        // Проверяем: либо это телепорт, либо стартовая точка
        bool isTeleport = PlayerPrefs.GetString("SpawnPoint") == pointId;
        bool isDefault = isDefaultSpawn && PlayerPrefs.GetString("SpawnPoint") == "";

        if (isTeleport || isDefault)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
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