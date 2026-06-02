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

        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject newAttack = Instantiate(attackPrefab, spawnPosition, Quaternion.identity);
        newAttack.transform.SetParent(canvas.transform, false);
    }
}