using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Vector2 movementInput;
    private Rigidbody2D rb;
    private PlayerInput playerInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Configurar input manualmente sin depender de eventos del Inspector
        playerInput = gameObject.AddComponent<PlayerInput>();
        SetupInputActions();

        Debug.Log("Sistema de movimiento inicializado");
    }

    private void SetupInputActions()
    {
        // Crear acciones de input program�ticamente
        var inputActions = new InputActionMap("Gameplay");

        // Acci�n de movimiento
        var moveAction = inputActions.AddAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // Suscribirse al evento de movimiento
        moveAction.performed += context => movementInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => movementInput = Vector2.zero;

        // Habilitar las acciones
        inputActions.Enable();

        // Asignar al PlayerInput
        playerInput.actions = inputActions.asset;
        playerInput.defaultActionMap = "Gameplay";
    }

    private void Update()
    {
        // Debug del input
        if (movementInput != Vector2.zero)
        {
            Debug.Log($"Input recibido: {movementInput}");
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
    }

    private void MovePlayer()
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D no encontrado!");
            return;
        }

        Vector2 movement = movementInput * moveSpeed;
        rb.linearVelocity = movement;

        if (movement != Vector2.zero)
        {
            Debug.Log($"Movimiento aplicado - Velocidad: {rb.linearVelocity}");
        }
    }

    private void RotatePlayer()
    {
        if (movementInput != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public Vector2 GetCurrentVelocity()
    {
        return rb != null ? rb.linearVelocity : Vector2.zero;
    }

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.actions?.Disable();
        }
    }
}