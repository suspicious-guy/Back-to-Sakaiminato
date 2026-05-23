using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
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
            healthText.text = $"Здоровье: {currentHealth}";
        }
    }
    
    void Die()
    {
        Debug.Log("Игрок погиб!");
    }
}