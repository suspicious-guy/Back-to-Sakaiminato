using UnityEngine;

public class FightButton : MonoBehaviour
{
    [Header("Урон")]
    public float damage = 10f;

    [Header("Ссылка на врага")]
    public EnemyHealth enemy;

    public void OnFightButtonClick()
    {
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }
}