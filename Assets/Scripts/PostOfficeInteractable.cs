using UnityEngine;

public class PostOfficeInteractable : Interactable
{
    [Header("Post Office Settings")]
    public string officeName = "Oficina de Correos Abismal";
    public float interactionCooldown = 1f;

    [Header("Visual Feedback")]
    public GameObject interactionPrompt;
    public ParticleSystem postOfficeEffects;

    private bool canInteract = true;
    private bool playerInRange = false;

    void Start()
    {
        // Ocultar prompt al inicio
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        HandleInteractionInput();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowInteractionPrompt();
            Debug.Log($"🏣 Jugador cerca de {officeName}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideInteractionPrompt();
            Debug.Log($"🏣 Jugador se alejó de {officeName}");
        }
    }

    private void HandleInteractionInput()
    {
        if (playerInRange && canInteract && Input.GetKeyDown(KeyCode.E))
        {
            InteractWithPostOffice();
        }
    }

    private void InteractWithPostOffice()
    {
        if (!canInteract) return;

        Debug.Log($"🏣 Interactuando con {officeName}");

        // Efectos visuales
        if (postOfficeEffects != null)
            postOfficeEffects.Play();

        // Abrir menú de misiones
        OpenMissionMenu();

        // Cooldown de interacción
        StartCoroutine(InteractionCooldown());

        OnInteractSuccess();
    }

    private System.Collections.IEnumerator InteractionCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }

    private void ShowInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    private void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void OpenMissionMenu()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenMissionMenu();
            Debug.Log("📋 Menú de misiones abierto");
        }
        else
        {
            Debug.LogError("❌ UIManager no encontrado");
        }
    }

    public override void Interact(GameObject player)
    {
        // Implementación requerida por la clase base
        InteractWithPostOffice();
    }

    public override bool CanInteract()
    {
        return canInteract && playerInRange;
    }
}