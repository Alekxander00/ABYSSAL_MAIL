using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Component References")]
    public  PlayerMovement movement;
    public PlayerHealth health;
    public PlayerInteraction interaction;
    public PlayerAnimation playerAnimation;

    private void Awake()
    {
        // Si no están asignados en el inspector, buscarlos automáticamente
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (health == null) health = GetComponent<PlayerHealth>();
        if (interaction == null) interaction = GetComponent<PlayerInteraction>();
        if (playerAnimation == null) playerAnimation = GetComponent<PlayerAnimation>();

        // Verificar que todos los componentes están presentes
        if (movement == null) Debug.LogError("PlayerMovement no encontrado en " + gameObject.name);
        if (health == null) Debug.LogError("PlayerHealth no encontrado en " + gameObject.name);
        if (interaction == null) Debug.LogError("PlayerInteraction no encontrado en " + gameObject.name);
        if (playerAnimation == null) Debug.LogError("PlayerAnimation no encontrado en " + gameObject.name);

        InitializePlayer();
    }

    private void InitializePlayer()
    {
        // Suscribirse a eventos
        health.OnPlayerDeath += HandlePlayerDeath;
        health.OnHealthChanged += HandleHealthChanged;
    }

    private void HandlePlayerDeath()
    {
        // Deshabilitar controles al morir
        if (movement != null) movement.enabled = false;
        if (interaction != null) interaction.enabled = false;

        Debug.Log("Player controls disabled - Player died");
    }

    private void HandleHealthChanged(float healthPercentage)
    {
        Debug.Log($"Health changed: {healthPercentage * 100}%");
    }

    private void OnDestroy()
    {
        // Limpiar suscripciones
        if (health != null)
        {
            health.OnPlayerDeath -= HandlePlayerDeath;
            health.OnHealthChanged -= HandleHealthChanged;
        }
    }
}