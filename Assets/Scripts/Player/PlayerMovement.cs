using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Vector2 movementInput;
    private Rigidbody2D rb;
    private InputAction moveAction;
    private bool isPaused = false;
    private bool isInitialized = false;
    private Coroutine subscriptionCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("❌ Rigidbody2D no encontrado en PlayerMovement");
            // Intentar agregar uno automáticamente
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 1f;
            rb.angularDamping = 0.05f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log("🔨 Rigidbody2D agregado automáticamente");
        }

        // Configurar input
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        moveAction.Enable();

        // Iniciar proceso de suscripción al GameManager
        subscriptionCoroutine = StartCoroutine(SubscribeToGameManager());

        isInitialized = true;
        Debug.Log("✅ PlayerMovement inicializado - Movimiento básico funcionando");
    }

    private IEnumerator SubscribeToGameManager()
    {
        Debug.Log("🔄 PlayerMovement intentando suscribirse al GameManager...");

        int maxAttempts = 10;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            attempts++;

            // Usar el método GetOrCreateInstance para asegurar que existe
            GameManager gameManager = GameManager.GetOrCreateInstance();

            if (gameManager != null && gameManager.IsReady())
            {
                // Suscribirse al evento
                gameManager.OnGameStateChanged += OnGameStateChanged;
                Debug.Log($"✅ PlayerMovement suscrito a GameManager (intento {attempts})");
                yield break; // Salir de la corrutina
            }
            else
            {
                Debug.Log($"⏳ Esperando GameManager... intento {attempts}/{maxAttempts}");
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Si llegamos aquí, fallaron todos los intentos
        Debug.LogWarning("⚠️ PlayerMovement no pudo suscribirse al GameManager después de " + maxAttempts + " intentos");
        Debug.Log("🎮 El movimiento seguirá funcionando, pero sin integración con el sistema de pausa del GameManager");
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Leer input siempre
        movementInput = moveAction.ReadValue<Vector2>();

        // Si no hay GameManager, siempre permitir movimiento
        if (GameManager.Instance == null || !GameManager.Instance.IsReady())
        {
            // Movimiento sin restricciones de pausa
            return;
        }

        // Aplicar lógica de pausa solo si GameManager está disponible
        if (isPaused)
        {
            movementInput = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (!isInitialized) return;
        if (rb == null) return;

        // Aplicar movimiento
        MovePlayer();

        // Rotar solo si hay movimiento
        if (movementInput != Vector2.zero)
        {
            RotatePlayer();
        }
    }

    private void MovePlayer()
    {
        Vector2 movement = movementInput * moveSpeed;
        rb.linearVelocity = movement;

        // Debug opcional del movimiento
        if (movement != Vector2.zero && Time.frameCount % 60 == 0)
        {
            Debug.Log($"🎮 Movimiento: {movementInput}, Velocidad: {rb.linearVelocity.magnitude:F2}");
        }
    }

    private void RotatePlayer()
    {
        float targetAngle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    private void OnGameStateChanged(GameState newState)
    {
        if (!isInitialized) return;

        // Actualizar estado de pausa
        isPaused = (newState == GameState.Paused);

        if (isPaused)
        {
            // Forzar detención inmediata
            movementInput = Vector2.zero;
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            Debug.Log("⏸️ PlayerMovement: JUEGO EN PAUSA - Movimiento desactivado");
        }
        else
        {
            Debug.Log("▶️ PlayerMovement: JUEGO REANUDADO - Movimiento activado");
        }
    }

    public Vector2 GetCurrentVelocity()
    {
        return rb != null ? rb.linearVelocity : Vector2.zero;
    }

    private void OnDestroy()
    {
        // Limpiar input action
        moveAction?.Disable();
        moveAction?.Dispose();

        // Detener corrutina si está en ejecución
        if (subscriptionCoroutine != null)
        {
            StopCoroutine(subscriptionCoroutine);
        }

        // Limpiar suscripción
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        Debug.Log("🗑️ PlayerMovement destruido y recursos liberados");
    }

    // Método para forzar la pausa manualmente (útil para testing)
    public void SetPauseManually(bool pause)
    {
        isPaused = pause;
        if (pause)
        {
            movementInput = Vector2.zero;
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
    }
}