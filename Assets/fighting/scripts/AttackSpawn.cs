using UnityEngine;

public class AttackSpawn : MonoBehaviour
{
    public GameObject attackPrefab;
    public Vector3 spawnPosition = new Vector3(0, 500, 0);

    void Start()
    {
        SpawnAttack();
    }

    void SpawnAttack()
    {
        if (attackPrefab == null) return;

        // ищем Canvas в той же сцене что и AttackSpawn
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        Canvas canvas = null;
        foreach (var c in allCanvases)
        {
            if (c.gameObject.scene == gameObject.scene)
            {
                canvas = c;
                break;
            }
        }

        if (canvas == null) return;

        GameObject newAttack = Instantiate(attackPrefab, spawnPosition, Quaternion.identity);
        newAttack.transform.SetParent(canvas.transform, false); // false важен
    }
}