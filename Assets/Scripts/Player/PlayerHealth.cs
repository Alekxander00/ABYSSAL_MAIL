using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    // Eventos para que otros sistemas escuchen
    public event Action<float> OnHealthChanged;
    public event Action OnPlayerDeath;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Disparar evento
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Disparar evento
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    private void Die()
    {
        OnPlayerDeath?.Invoke();

        // Aquí manejarías la muerte del jugador
        Debug.Log("Player died!");

        // Temporal: reiniciar escena
        // UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}