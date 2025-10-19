using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 8f;

    [Header("Component References")]
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private Vector2 movementInput;
    private Keyboard keyboard;

    void Start()
    {
        // Obtener referencias
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Configurar física
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        keyboard = Keyboard.current;
    }

    void Update()
    {
        HandleInput();
        HandleAnimation();
        HandleSmartRotation();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleInput()
    {
        movementInput = Vector2.zero;

        // Input System directo
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            movementInput.y += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            movementInput.y -= 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            movementInput.x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            movementInput.x += 1f;

        // Normalizar para movimiento diagonal
        if (movementInput.magnitude > 1f)
            movementInput.Normalize();
    }

    private void HandleMovement()
    {
        if (movementInput.magnitude > 0.1f)
        {
            rb.linearVelocity = movementInput * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandleSmartRotation()
    {
        if (movementInput.magnitude > 0.1f)
        {
            // Calcular el ángulo de movimiento
            float targetAngle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;

            // CORRECCIÓN: Ajustar ángulo para direcciones izquierda
            if (movementInput.x < 0)
            {
                // Cuando va hacia la izquierda, ajustamos el ángulo para que no quede boca abajo
                targetAngle += 180f;
            }

            // Aplicar rotación suave
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Flip horizontal basado en dirección
            spriteRenderer.flipX = movementInput.x < 0;
        }
        else
        {
            // Volver suavemente a rotación neutral cuando está quieto
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, rotationSpeed * Time.deltaTime);
            spriteRenderer.flipX = false;
        }
    }

    private void HandleAnimation()
    {
        if (animator != null)
        {
            bool isMoving = movementInput.magnitude > 0.1f;
            animator.SetBool("isMoving", isMoving);
        }
    }
}