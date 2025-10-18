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
    public int currentDay = 1;

    [Header("Player Progress")]
    public int completedMissionsCount = 0;
    public int totalEarnings = 0;

    [Header("Available Missions")]
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

    // Método estático para obtener la instancia (auto-crea si no existe)
    public static GameManager GetOrCreateInstance()
    {
        if (Instance == null)
        {
            // Buscar en la escena
            Instance = FindObjectOfType<GameManager>();

            // Si no existe, crear uno nuevo
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
        // Singleton pattern mejorado
        if (Instance != null && Instance != this)
        {
            Debug.Log("⚠️ Destruyendo GameManager duplicado");
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

        // Buscar player si no está asignado
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
                if (player == null)
                {
                    Debug.LogWarning("⚠️ Player no encontrado. Se buscará automáticamente.");
                }
            }
        }

        // Estado inicial del juego
        ChangeGameState(GameState.Exploring);

        isInitialized = true;

        Debug.Log("✅ GameManager completamente inicializado y listo");
    }

    private void Start()
    {
        // Verificación final
        if (!isInitialized)
        {
            InitializeGame();
        }

        // Buscar player nuevamente en Start por si no se encontró en Awake
        if (player == null)
        {
            FindPlayer();
        }
    }

    private void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        if (player != null)
        {
            Debug.Log("✅ Player encontrado: " + player.name);
        }
        else
        {
            Debug.LogWarning("⚠️ Player sigue sin encontrarse. Algunas funciones pueden no trabajar correctamente.");
        }
    }

    #region Game State Management
    public void ChangeGameState(GameState newState)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("⚠️ GameManager no inicializado al cambiar estado");
            return;
        }

        GameState previousState = currentGameState;
        currentGameState = newState;

        // Aplicar lógica específica por estado
        switch (newState)
        {
            case GameState.Paused:
                SetPause(true);
                break;
            default:
                SetPause(false);
                break;
        }

        // Disparar evento de manera segura
        OnGameStateChanged?.Invoke(newState);

        Debug.Log($"🔄 Estado del juego: {previousState} -> {newState}");
    }

    private void SetPause(bool pause)
    {
        if (!isInitialized) return;

        if (pause)
        {
            Time.timeScale = 0f;
            Debug.Log("⏸️ TIME SCALE = 0 (JUEGO PAUSADO)");
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("▶️ TIME SCALE = 1 (JUEGO REANUDADO)");
        }
    }
    #endregion

    #region Mission Management
    public void AcceptMission(Mission mission)
    {
        if (!isInitialized) return;

        if (availableMissions.Contains(mission) && !activeMissions.Contains(mission))
        {
            availableMissions.Remove(mission);
            activeMissions.Add(mission);
            mission.missionStatus = MissionStatus.InProgress;

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

            OnMissionCompleted?.Invoke(mission);

            Debug.Log($"🎉 Misión completada: {mission.missionName}");
            Debug.Log($"💰 +{mission.moneyReward} monedas");
        }
    }

    public bool CanAcceptMission(Mission mission)
    {
        if (!isInitialized) return false;

        return availableMissions.Contains(mission) && activeMissions.Count < 3;
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

    [ContextMenu("Test Pause Game")]
    private void TestPauseGame()
    {
        if (!isInitialized) return;
        ChangeGameState(GameState.Paused);
    }

    [ContextMenu("Test Resume Game")]
    private void TestResumeGame()
    {
        if (!isInitialized) return;
        ChangeGameState(GameState.Exploring);
    }

    [ContextMenu("Print Game Status")]
    private void PrintGameStatus()
    {
        if (!isInitialized)
        {
            Debug.Log("❌ GameManager no está inicializado");
            return;
        }

        Debug.Log($"🎮 ESTADO DEL JUEGO:");
        Debug.Log($"💰 Dinero: {totalEarnings}");
        Debug.Log($"✅ Misiones completadas: {completedMissionsCount}");
        Debug.Log($"📋 Misiones activas: {activeMissions.Count}");
        Debug.Log($"📂 Misiones disponibles: {availableMissions.Count}");
        Debug.Log($"🎒 Items en inventario: {playerInventory.Count}");
        Debug.Log($"🔄 Estado actual: {currentGameState}");
        Debug.Log($"⏰ TimeScale: {Time.timeScale}");
        Debug.Log($"🎯 Player asignado: {player != null}");
    }
    #endregion

    // Método público para verificar si está inicializado
    public bool IsReady()
    {
        return isInitialized;
    }

    // Método para forzar la inicialización
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