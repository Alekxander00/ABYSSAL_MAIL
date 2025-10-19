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

    [Header("Debug")]
    public bool enableDebug = true;

    private Mission myMission;
    private bool isProcessing = false;
    private Coroutine currentDialogueCoroutine;

    // ✅ NUEVO: Asegurar que el globo esté desactivado al inicio
    void Start()
    {
        ForceHideDialogueAtStart();
    }

    private void ForceHideDialogueAtStart()
    {
        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(false);
            if (enableDebug) Debug.Log($"🔧 {npcName}: Globo desactivado al inicio");
        }
        else if (enableDebug)
        {
            Debug.LogWarning($"⚠️ {npcName}: dialogueBubble no asignado en Inspector");
        }
    }

    public override void Interact(GameObject player)
    {
        if (!CanInteract() || isProcessing)
        {
            if (enableDebug) Debug.Log($"⏳ {npcName}: No se puede interactuar (procesando: {isProcessing})");
            return;
        }

        if (enableDebug) Debug.Log($"🎮 {npcName}: Iniciando interacción");

        isProcessing = true;

        // Ocultar diálogo anterior antes de mostrar uno nuevo
        HideDialogueBubble();

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
        if (enableDebug) Debug.Log($"🔄 {npcName}: Procesamiento reiniciado");
    }

    private void FindMyMission()
    {
        if (myMission == null && GameManager.Instance != null && GameManager.Instance.IsReady())
        {
            myMission = GameManager.Instance.GetMissionForNPC(npcName);
            if (enableDebug && myMission != null)
                Debug.Log($"🔍 {npcName}: Misión encontrada - {myMission.missionName}");
        }
    }

    private void HandleMissionInteraction()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            if (enableDebug) Debug.LogError("❌ GameManager no encontrado");
            return;
        }

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
        if (enableDebug) Debug.Log($"🎉 {npcName}: ¡Entrega completada!");

        PlayDeliveryEffects();

        // Actualizar UI
        UIManager.Instance?.UpdateHUD();
    }

    private void ShowGreetingDialogue()
    {
        string dialogue = GetRandomDialogue(greetingDialogues);
        ShowDialogueBubble(dialogue);
        if (enableDebug) Debug.Log($"{npcName}: {dialogue}");
    }

    private void ShowDeliveryDialogue()
    {
        string dialogue = GetRandomDialogue(deliveryDialogues);
        ShowDialogueBubble(dialogue);
        if (enableDebug) Debug.Log($"{npcName}: {dialogue}");
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
    }

    private void ShowDialogueBubble(string text)
    {
        if (dialogueBubble == null || dialogueText == null)
        {
            if (enableDebug) Debug.LogError($"❌ {npcName}: dialogueBubble o dialogueText no asignados en el Inspector");
            return;
        }

        // Cancelar diálogo anterior si existe
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }

        // Mostrar nuevo diálogo
        dialogueText.text = text;
        dialogueBubble.SetActive(true);
        if (enableDebug) Debug.Log($"💬 {npcName}: Mostrando diálogo - {text}");

        // Iniciar temporizador para ocultar
        currentDialogueCoroutine = StartCoroutine(HideDialogueAfterDelay());
    }

    private IEnumerator HideDialogueAfterDelay()
    {
        yield return new WaitForSeconds(dialogueDuration);
        HideDialogueBubble();
    }

    public void HideDialogueBubble()
    {
        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(false);
            if (enableDebug) Debug.Log($"🔇 {npcName}: Diálogo ocultado");
        }

        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
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

    // Para debug en tiempo real
    private void Update()
    {
        // Ocultar diálogo si el jugador se aleja (opcional)
        if (dialogueBubble != null && dialogueBubble.activeSelf)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance > 5f) // Si el jugador está muy lejos
                {
                    HideDialogueBubble();
                }
            }
        }
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

    [ContextMenu("Test Dialogue")]
    public void TestDialogue()
    {
        ShowDialogueBubble("Este es un diálogo de prueba");
    }
}