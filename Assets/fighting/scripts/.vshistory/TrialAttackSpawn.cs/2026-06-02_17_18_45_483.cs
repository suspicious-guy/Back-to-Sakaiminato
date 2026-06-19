using UnityEngine;

public class TrialAttackSpawn : MonoBehaviour
{
    [Header("Префаб атаки")]
    public GameObject attackPrefab;

    [Header("Интервал между атаками")]
    public float interval = 5f;

    [Header("Позиция спавна")]
    public Vector3 spawnPosition = new Vector3(0, 500, 0);

    private float timer;
    private Canvas canvas;

    void Start()
    {
        //ян для лучшего поиска canvas
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        canvas = null;
        foreach (var c in allCanvases)
        {
            if (c.gameObject.scene == gameObject.scene)
            {
                canvas = c;
                break;
            }
        }
    }

    void Update()
    {
        EnemyHealth enemyHealth = FindObjectOfType<EnemyHealth>();
        if (enemyHealth == null || enemyHealth.currentHealth <= 0) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnAttack();
            timer = interval;
        }
    }

    void SpawnAttack()
    {
        if (attackPrefab == null) return;
        if (canvas == null) return;

        GameObject newAttack = Instantiate(attackPrefab, spawnPosition, Quaternion.identity);
        newAttack.transform.SetParent(canvas.transform, false);
    }
}