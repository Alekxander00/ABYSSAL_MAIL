using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "Abyssal Mail/Mission")]
public class Mission : ScriptableObject
{
    [Header("Mission Info")]
    public string missionName;
    [TextArea] public string description;
    public MissionType missionType;

    [Header("Requirements")]
    public int requiredReputation = 0;
    public Item requiredItem; // Para misiones de entrega
    public string targetNPCName; // NPC que recibe la entrega

    [Header("Rewards")]
    public int moneyReward = 50;
    public int reputationReward = 10;
    public Item[] itemRewards;

    [Header("Progress")]
    public MissionStatus missionStatus = MissionStatus.Available;

    // Para misiones de colección
    public Item itemToCollect;
    public int requiredQuantity = 1;
    public int currentQuantity = 0;
}

public enum MissionType
{
    Delivery,    // Entregar item a NPC
    Collection,  // Recolectar items
    Exploration  // Visitar ubicación
}

public enum MissionStatus
{
    Available,
    InProgress,
    Completed,
    Failed
}