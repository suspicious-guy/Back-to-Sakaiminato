using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EnemyHealth : MonoBehaviour
{
    [Header("Здоровье")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public TextMeshProUGUI healthText;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"Сиримэ: {currentHealth}";
        }
    }

    void Die()
    {
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}
