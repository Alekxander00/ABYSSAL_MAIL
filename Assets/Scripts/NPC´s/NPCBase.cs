using UnityEngine;
using TMPro;
using System.Collections;

public class NPCBase : Interactable
{
    [Header("NPC Settings")]
    public string npcName;

    [Header("Dialogue System")]
    public string[] greetingDialogues;
    public string[] deliveryDialogues;
    public string[] completionDialogues;
    public string[] idleDialogues;

    [Header("Visual Feedback")]
    public GameObject dialogueBubble;
    public TextMeshProUGUI dialogueText;
    public float dialogueDuration = 3f;

    [Header("Effects")]
    public ParticleSystem deliveryEffect;
    public AudioClip deliverySound;

    private Mission myMission;
    private bool isProcessing = false;

    public override void Interact(GameObject player)
    {
        if (!CanInteract() || isProcessing) return;

        isProcessing = true;

        FindMyMission();

        if (myMission != null)
        {
            HandleMissionInteraction();
        }
        else
        {
            ShowGreetingDialogue();
        }

        OnInteractSuccess();

        // Reset processing after a short delay
        StartCoroutine(ResetProcessing());
    }

    private IEnumerator ResetProcessing()
    {
        yield return new WaitForSeconds(0.5f);
        isProcessing = false;
    }

    private void FindMyMission()
    {
        if (myMission == null && GameManager.Instance != null && GameManager.Instance.IsReady())
        {
            myMission = GameManager.Instance.GetMissionForNPC(npcName);
        }
    }

    private void HandleMissionInteraction()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;

        if (gameManager.activeMissions.Contains(myMission))
        {
            if (myMission.requiredItem != null && gameManager.HasItem(myMission.requiredItem))
            {
                CompleteDelivery();
            }
            else
            {
                ShowDeliveryDialogue();
            }
        }
        else if (gameManager.completedMissions.Contains(myMission))
        {
            ShowCompletionDialogue();
        }
        else
        {
            ShowGreetingDialogue();
        }
    }

    private void CompleteDelivery()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;

        // Completar la misión
        gameManager.CompleteMission(myMission);

        // Remover el item de entrega
        if (myMission.requiredItem != null)
        {
            gameManager.RemoveItemFromInventory(myMission.requiredItem);
        }

        string completionDialogue = GetRandomDialogue(completionDialogues);
        ShowDialogueBubble(completionDialogue);
        Debug.Log($"🎉 {npcName}: ¡Entrega completada!");

        PlayDeliveryEffects();

        // Actualizar UI
        UIManager.Instance?.UpdateHUD();
    }

    private void ShowGreetingDialogue()
    {
        string dialogue = GetRandomDialogue(greetingDialogues);
        ShowDialogueBubble(dialogue);
        Debug.Log($"{npcName}: {dialogue}");
    }

    private void ShowDeliveryDialogue()
    {
        string dialogue = GetRandomDialogue(deliveryDialogues);
        ShowDialogueBubble(dialogue);
        Debug.Log($"{npcName}: {dialogue}");
    }

    private void ShowCompletionDialogue()
    {
        string dialogue = GetRandomDialogue(idleDialogues);
        ShowDialogueBubble(dialogue);
    }

    private void PlayDeliveryEffects()
    {
        // Efectos de partículas
        if (deliveryEffect != null)
        {
            deliveryEffect.Play();
        }

        // Sonido de entrega
        if (deliverySound != null)
        {
            AudioSource.PlayClipAtPoint(deliverySound, transform.position);
        }

        // Efectos específicos por NPC
        switch (npcName.ToLower())
        {
            case "pez globo":
                // Efectos románticos
                break;
            case "cangrejo robot":
                // Efectos mecánicos
                break;
            case "pulpo sabio":
                // Efectos de sabiduría
                break;
            case "mantarraya":
                // Efectos de aventura
                break;
        }
    }

    private void ShowDialogueBubble(string text)
    {
        if (dialogueBubble != null && dialogueText != null)
        {
            dialogueText.text = text;
            dialogueBubble.SetActive(true);
            StartCoroutine(HideDialogueAfterDelay());
        }
    }

    private IEnumerator HideDialogueAfterDelay()
    {
        yield return new WaitForSeconds(dialogueDuration);
        HideDialogueBubble();
    }

    private void HideDialogueBubble()
    {
        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(false);
        }
    }

    private string GetRandomDialogue(string[] dialogues)
    {
        if (dialogues == null || dialogues.Length == 0)
            return $"{npcName} mira curiosamente...";

        return dialogues[Random.Range(0, dialogues.Length)];
    }

    public override bool CanInteract()
    {
        return true;
    }

    [ContextMenu("Test Delivery")]
    public void TestDelivery()
    {
        FindMyMission();
        if (myMission != null && GameManager.Instance != null)
        {
            GameManager gameManager = GameManager.Instance;

            // Asegurar que la misión esté disponible y aceptada
            if (!gameManager.availableMissions.Contains(myMission) && !gameManager.activeMissions.Contains(myMission))
            {
                gameManager.AddMissionToAvailable(myMission);
            }

            if (!gameManager.activeMissions.Contains(myMission))
            {
                gameManager.AcceptMission(myMission);
            }

            // Asegurar que el jugador tenga el item
            if (myMission.requiredItem != null && !gameManager.HasItem(myMission.requiredItem))
            {
                gameManager.AddItemToInventory(myMission.requiredItem);
            }

            CompleteDelivery();
        }
        else
        {
            Debug.LogWarning($"No se pudo encontrar misión para {npcName}");
        }
    }

    [ContextMenu("Find My Mission")]
    public void FindMyMissionDebug()
    {
        FindMyMission();
        if (myMission != null)
        {
            Debug.Log($"🔍 Misión encontrada para {npcName}: {myMission.missionName}");
        }
        else
        {
            Debug.LogWarning($"❌ No se encontró misión para {npcName}");
        }
    }
}