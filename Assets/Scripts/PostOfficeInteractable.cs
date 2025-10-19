using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
    private Keyboard keyboard;

    void Start()
    {
        keyboard = Keyboard.current;

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
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideInteractionPrompt();
        }
    }

    private void HandleInteractionInput()
    {
        if (playerInRange && canInteract && keyboard.eKey.wasPressedThisFrame)
        {
            InteractWithPostOffice();
        }
    }

    private void InteractWithPostOffice()
    {
        if (!canInteract) return;

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
        }
    }

    public override void Interact(GameObject player)
    {
        InteractWithPostOffice();
    }

    public override bool CanInteract()
    {
        return canInteract && playerInRange;
    }

    // Método para que el UIManager pueda notificar cuando se cierra el menú
    public void OnMissionMenuClosed()
    {
        HideInteractionPrompt();
    }
}