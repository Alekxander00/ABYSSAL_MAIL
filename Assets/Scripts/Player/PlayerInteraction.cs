using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 2f;
    public LayerMask interactableLayer = 1;

    private Interactable currentInteractable;
    private bool canInteract = true;
    private InputAction interactAction;

    private void Awake()
    {
        // Crear acción de interacción DIRECTAMENTE, sin depender de PlayerInput
        interactAction = new InputAction("Interact", InputActionType.Button);
        interactAction.AddBinding("<Keyboard>/e");  // Tecla E
        interactAction.AddBinding("<Keyboard>/f");  // Tecla F alternativa
        interactAction.Enable();

        Debug.Log("✅ Sistema de interacciones inicializado - Presiona E o F");
    }

    private void Update()
    {
        FindNearbyInteractables();
        HandleInteractionInput();
    }

    private void FindNearbyInteractables()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);

        Interactable closestInteractable = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hitCollider in hitColliders)
        {
            Interactable interactable = hitCollider.GetComponent<Interactable>();
            if (interactable != null && interactable.CanInteract())
            {
                float distance = Vector2.Distance(transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        currentInteractable = closestInteractable;

        // Debug visual en consola
        if (currentInteractable != null)
        {
            Debug.Log($"🎯 Objeto interactuable cerca: {currentInteractable.gameObject.name}");
        }
    }

    private void HandleInteractionInput()
    {
        if (!canInteract) return;

        // Usar Input System correctamente
        if (interactAction.triggered && currentInteractable != null)
        {
            currentInteractable.Interact(gameObject);
            Debug.Log($"✅ INTERACCIÓN EXITOSA con: {currentInteractable.gameObject.name}");
        }
    }

    public void EnableInteraction() => canInteract = true;
    public void DisableInteraction() => canInteract = false;

    private void OnDestroy()
    {
        interactAction?.Disable();
    }

    // Debug visual en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = currentInteractable != null ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}