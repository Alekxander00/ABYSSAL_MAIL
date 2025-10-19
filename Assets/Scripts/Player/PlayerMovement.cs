using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Component References")]
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private Vector2 movementInput;
    private Keyboard keyboard;
    private Vector2 lastNonZeroInput = Vector2.right; // Por defecto mirando a la derecha

    void Start()
    {
        // Obtener referencias automáticamente
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
        HandleConsistentRotation();
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

        // Guardar la última dirección no-cero para cuando esté quieto
        if (movementInput.magnitude > 0.1f)
        {
            lastNonZeroInput = movementInput;
        }
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

    private void HandleConsistentRotation()
    {
        Vector2 directionToUse = movementInput.magnitude > 0.1f ? movementInput : lastNonZeroInput;

        // SOLUCIÓN: Sistema de 8 direcciones fijas
        float targetAngle = 0f;
        bool flipX = false;

        // Determinar dirección principal con umbrales
        float absX = Mathf.Abs(directionToUse.x);
        float absY = Mathf.Abs(directionToUse.y);

        if (absX > absY)
        {
            // Dirección principalmente horizontal
            if (directionToUse.x > 0)
            {
                targetAngle = 0f;   // Derecha
                flipX = false;
            }
            else
            {
                targetAngle = 0f;   // Izquierda (usamos flip en lugar de rotación)
                flipX = true;
            }
        }
        else
        {
            // Dirección principalmente vertical
            if (directionToUse.y > 0)
            {
                targetAngle = 90f;  // Arriba
                flipX = false;
            }
            else
            {
                targetAngle = -90f; // Abajo
                flipX = false;
            }
        }

        // Para diagonales, usar ángulos intermedios
        if (absX > 0.3f && absY > 0.3f)
        {
            if (directionToUse.y > 0)
            {
                if (directionToUse.x > 0)
                    targetAngle = 45f;   // Diagonal arriba-derecha
                else
                    targetAngle = 135f;  // Diagonal arriba-izquierda
            }
            else
            {
                if (directionToUse.x > 0)
                    targetAngle = -45f;  // Diagonal abajo-derecha
                else
                    targetAngle = -135f; // Diagonal abajo-izquierda
            }
            flipX = false; // No usar flip en diagonales, la rotación ya maneja la dirección
        }

        // Aplicar rotación y flip
        transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        spriteRenderer.flipX = flipX;
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