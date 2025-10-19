using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBehavior : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float speed = 3f;
    public float detectionRange = 5f;
    public float attackRange = 3f;
    public float damage = 34f;
    public float attackCooldown = 3f;

    private Transform player;
    private PlayerHealth playerHealth;
    private bool playerDead = false;
    private bool canAttack = true;
    private bool isOnCooldown = false;

    [Header("Movimiento y detección")]
    public float rangoDeteccion = 10f;
    public float velocidadMin = 2f;
    public float velocidadMax = 6f;
    public float stoppingDistance = 0.5f;
    private bool jugadorMuerto;

    private Rigidbody2D rb;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.OnPlayerDeath += OnPlayerDeathHandler;
        }
    }

    private void Update()
    {
        if (player == null || playerDead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Si está en cooldown, se queda quieto
        if (isOnCooldown)
        {
            StopMovement();
            return;
        }

        // Solo persigue si está dentro del rango de detección
        if (distance <= rangoDeteccion)
        {
            MoveTowardsPlayer(distance);
        }
        else
        {
            StopMovement();
        }
    }

    private void MoveTowardsPlayer(float distance)
    {
        // Calcular velocidad en función de qué tan cerca está el jugador
        float t = Mathf.Clamp01(1 - (distance / rangoDeteccion));
        float currentSpeed = Mathf.Lerp(velocidadMin, velocidadMax, t);

        // Dirección y movimiento
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * currentSpeed;

        // Activar animación de movimiento
        if (animator != null)
            animator.SetBool("isMoving", true);
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
        if (animator != null)
            animator.SetBool("isMoving", false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Solo atacar si colisiona con el jugador, puede atacar y no está muerto
        if (!canAttack || playerDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;
        isOnCooldown = true;
        StopMovement();

        // Animación de ataque
        if (animator != null)
            animator.SetTrigger("attack");

        // Aplicar daño si el jugador sigue vivo
        if (playerHealth != null && !playerDead)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log("Enemigo golpeó al jugador. Vida restante: " + playerHealth.currentHealth);
        }

        // Esperar cooldown (quieto durante ese tiempo)
        yield return new WaitForSeconds(attackCooldown);

        isOnCooldown = false;
        canAttack = true;
    }

    private void OnPlayerDeathHandler()
    {
        playerDead = true;
        canAttack = false;
        isOnCooldown = false;
        StopMovement();

        Debug.Log("Jugador muerto. Enemigo se detiene.");
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnPlayerDeath -= OnPlayerDeathHandler;
    }

    private void OnDrawGizmosSelected()
    {
        // Color del rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Color del rango de ataque (referencial)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}