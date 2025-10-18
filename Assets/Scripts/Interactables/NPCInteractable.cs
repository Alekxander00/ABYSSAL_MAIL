using UnityEngine;

public class NPCInteractable : Interactable
{
    [Header("NPC Settings")]
    public string npcName;
    public Mission offeredMission;
    public bool isMissionGiver = true;

    [Header("Dialogue")]
    [TextArea] public string[] dialogueLines;
    private int currentDialogueIndex = 0;

    public override void Interact(GameObject player)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager no encontrado!");
            return;
        }

        GameManager.Instance.ChangeGameState(GameState.InDialogue);

        ShowDialogue();

        if (isMissionGiver && offeredMission != null)
        {
            OfferMission();
        }

        OnInteractSuccess();
    }

    private void ShowDialogue()
    {
        if (dialogueLines.Length > 0)
        {
            string dialogue = dialogueLines[currentDialogueIndex];
            Debug.Log($"💬 {npcName}: {dialogue}");

            // Rotar diálogos
            currentDialogueIndex = (currentDialogueIndex + 1) % dialogueLines.Length;
        }
    }

    private void OfferMission()
    {
        if (offeredMission.missionStatus == MissionStatus.Available)
        {
            Debug.Log($"📋 {npcName} ofrece misión: {offeredMission.missionName}");
            Debug.Log($"📝 {offeredMission.description}");
            Debug.Log("Presiona 'A' para aceptar, 'R' para rechazar");

            // Aquí luego conectaremos con el UI
            GameManager.Instance.AcceptMission(offeredMission);
        }
        else if (offeredMission.missionStatus == MissionStatus.Completed)
        {
            Debug.Log($"✅ {npcName}: ¡Gracias por completar la misión!");
        }
    }

    public override bool CanInteract()
    {
        return true;
    }
}