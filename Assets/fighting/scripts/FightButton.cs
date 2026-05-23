using UnityEngine;

public class FightButton : MonoBehaviour
{
    [Header("Урон")]
    public float damage = 10f;

    public void OnFightButtonClick()
    {
        TrialEnemyHealth enemy = FindObjectOfType<TrialEnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }
}