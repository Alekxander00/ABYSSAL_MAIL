using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "Abyssal Mail/Mission")]
public class Mission : ScriptableObject
{
    [Header("Mission Info")]
    public string missionName;
    [TextArea] public string description;
    public MissionType missionType;

    [Header("Requirements")]
    public Item requiredItem;
    public string targetNPCName; // ← ESTE CAMPO ES CRÍTICO

    [Header("Rewards")]
    public int moneyReward = 50;
    public Item[] itemRewards;

    [Header("Progress")]
    public MissionStatus missionStatus = MissionStatus.Available;
}

public enum MissionType
{
    Delivery,
    Collection,
    Exploration
}

public enum MissionStatus
{
    Available,
    InProgress,
    Completed,
    Failed
}