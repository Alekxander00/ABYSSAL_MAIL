using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player & References")]
    public GameObject player;
    public Transform playerSpawnPoint;

    [Header("Game State")]
    public GameState currentGameState = GameState.Exploring;

    [Header("Player Progress")]
    public int completedMissionsCount = 0;
    public int totalEarnings = 0;

    [Header("Missions - Oficina de Correos")]
    public List<Mission> allMissions = new List<Mission>();
    public List<Mission> availableMissions = new List<Mission>();
    public List<Mission> activeMissions = new List<Mission>();
    public List<Mission> completedMissions = new List<Mission>();

    [Header("Inventory & Items")]
    public List<Item> playerInventory = new List<Item>();
    public int maxInventorySlots = 10;

    // Eventos del juego
    public System.Action<Mission> OnMissionAccepted;
    public System.Action<Mission> OnMissionCompleted;
    public System.Action<GameState> OnGameStateChanged;

    private bool isInitialized = false;

    // Método estático para obtener la instancia - CORREGIDO
    public static GameManager GetOrCreateInstance()
    {
        if (Instance == null)
        {
            // CORRECCIÓN: Usar FindFirstObjectByType en lugar de FindObjectOfType
            Instance = FindFirstObjectByType<GameManager>();

            if (Instance == null)
            {
                Debug.Log("🔨 Creando GameManager automáticamente...");
                GameObject gameManagerObject = new GameObject("GameManager_AutoCreated");
                Instance = gameManagerObject.AddComponent<GameManager>();
                DontDestroyOnLoad(gameManagerObject);
            }
        }
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeGame();
    }

    private void InitializeGame()
    {
        if (isInitialized) return;

        Debug.Log("🎮 Game Manager inicializado");

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        ChangeGameState(GameState.Exploring);
        InitializeAvailableMissions();

        isInitialized = true;
        Debug.Log("✅ GameManager completamente inicializado");
    }

    private void InitializeAvailableMissions()
    {
        if (availableMissions.Count == 0 && allMissions.Count > 0)
        {
            foreach (Mission mission in allMissions)
            {
                if (mission.missionStatus == MissionStatus.Available)
                {
                    availableMissions.Add(mission);
                }
            }
        }
        Debug.Log($"📋 Misiones disponibles: {availableMissions.Count}");
    }

    #region Mission Management - Oficina de Correos
    public void AcceptMission(Mission mission)
    {
        if (!isInitialized) return;

        if (availableMissions.Contains(mission) && !activeMissions.Contains(mission))
        {
            availableMissions.Remove(mission);
            activeMissions.Add(mission);
            mission.missionStatus = MissionStatus.InProgress;

            if (mission.requiredItem != null && !playerInventory.Contains(mission.requiredItem))
            {
                AddItemToInventory(mission.requiredItem);
            }

            OnMissionAccepted?.Invoke(mission);
            Debug.Log($"✅ Misión aceptada: {mission.missionName}");
        }
    }

    public void CompleteMission(Mission mission)
    {
        if (!isInitialized) return;

        if (activeMissions.Contains(mission))
        {
            activeMissions.Remove(mission);
            completedMissions.Add(mission);
            mission.missionStatus = MissionStatus.Completed;

            totalEarnings += mission.moneyReward;
            completedMissionsCount++;

            if (mission.itemRewards != null)
            {
                foreach (Item rewardItem in mission.itemRewards)
                {
                    AddItemToInventory(rewardItem);
                }
            }

            OnMissionCompleted?.Invoke(mission);
            Debug.Log($"🎉 Misión completada: {mission.missionName}");
            Debug.Log($"💰 +{mission.moneyReward} monedas");

            UIManager.Instance?.UpdateHUD();
        }
    }

    public bool CanAcceptMission(Mission mission)
    {
        if (!isInitialized) return false;
        return availableMissions.Contains(mission) && activeMissions.Count < 3;
    }

    public void AddMissionToAvailable(Mission mission)
    {
        if (!isInitialized) return;

        if (!availableMissions.Contains(mission))
        {
            availableMissions.Add(mission);
            mission.missionStatus = MissionStatus.Available;
            Debug.Log($"📋 Misión añadida a disponibles: {mission.missionName}");
        }
    }

    // MÉTODO NUEVO - Agregado para solucionar el error en NPCBase
    public Mission GetMissionForNPC(string npcName)
    {
        if (!isInitialized) return null;

        foreach (Mission mission in allMissions)
        {
            if (mission.targetNPCName == npcName)
            {
                return mission;
            }
        }
        return null;
    }
    #endregion

    #region Game State Management
    public void ChangeGameState(GameState newState)
    {
        if (!isInitialized) return;

        GameState previousState = currentGameState;
        currentGameState = newState;

        switch (newState)
        {
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            default:
                Time.timeScale = 1f;
                break;
        }

        OnGameStateChanged?.Invoke(newState);
        Debug.Log($"🔄 Estado del juego: {previousState} -> {newState}");
    }
    #endregion

    #region Inventory Management
    public bool AddItemToInventory(Item item)
    {
        if (!isInitialized) return false;

        if (playerInventory.Count < maxInventorySlots)
        {
            playerInventory.Add(item);
            Debug.Log($"🎒 Item añadido: {item.itemName}");
            return true;
        }
        else
        {
            Debug.LogWarning("🎒 Inventario lleno!");
            return false;
        }
    }

    public bool RemoveItemFromInventory(Item item)
    {
        if (!isInitialized) return false;

        if (playerInventory.Contains(item))
        {
            playerInventory.Remove(item);
            Debug.Log($"🎒 Item removido: {item.itemName}");
            return true;
        }
        return false;
    }

    public bool HasItem(Item item)
    {
        if (!isInitialized) return false;
        return playerInventory.Contains(item);
    }
    #endregion

    #region Debug & Testing Methods
    [ContextMenu("Test Add Money +50")]
    private void TestAddMoney()
    {
        if (!isInitialized) return;
        totalEarnings += 50;
        Debug.Log($"💰 Dinero total: {totalEarnings}");
    }

    [ContextMenu("Print Game Status")]
    private void PrintGameStatus()
    {
        if (!isInitialized) return;

        Debug.Log($"🎮 ESTADO DEL JUEGO:");
        Debug.Log($"💰 Dinero: {totalEarnings}");
        Debug.Log($"✅ Misiones completadas: {completedMissionsCount}");
        Debug.Log($"📋 Misiones activas: {activeMissions.Count}");
        Debug.Log($"📂 Misiones disponibles: {availableMissions.Count}");
        Debug.Log($"🎒 Items en inventario: {playerInventory.Count}");
    }
    #endregion

    public bool IsReady()
    {
        return isInitialized;
    }

    public void ForceInitialize()
    {
        if (!isInitialized)
        {
            InitializeGame();
        }
    }
}

public enum GameState
{
    Exploring,
    InDialogue,
    InMenu,
    Paused
}