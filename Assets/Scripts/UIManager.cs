using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Menu References")]
    public GameObject missionMenu;
    public GameObject pauseMenu;
    public GameObject hud;

    [Header("Mission Menu References - TMP")]
    public TextMeshProUGUI missionTitleText;
    public TextMeshProUGUI missionDescriptionText;
    public TextMeshProUGUI missionRewardText;
    public Button acceptMissionButton;
    public Button closeMissionButton;

    [Header("Pause Menu References")]
    public Button resumeButton;
    public Button quitButton;

    [Header("HUD References - TMP")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI missionCounterText;
    public TextMeshProUGUI moneyText;

    [Header("UI Settings")]
    public bool enableMissionMenu = true;
    public bool enablePauseMenu = true;

    private bool isMissionMenuOpen = false;
    private bool isPauseMenuOpen = false;
    private bool isInitialized = false;

    // Input Actions
    private InputAction missionMenuAction;
    private InputAction pauseMenuAction;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.Log("🗑️ Destruyendo UIManager duplicado");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("🖥️ UIManager inicializado");
    }

    private void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        SetupInputActions();

        // Asegurar que GameManager existe
        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager != null)
        {
            gameManager.ForceInitialize();
        }

        isInitialized = true;
        Debug.Log("✅ UIManager completamente inicializado");
    }

    private void Update()
    {
        // Actualizar HUD cada frame
        if (isInitialized)
        {
            UpdateHUD();
        }
    }

    private void InitializeUI()
    {
        // Verificar referencias críticas
        if (missionMenu == null) Debug.LogWarning("⚠️ MissionMenu no asignado en UIManager");
        if (pauseMenu == null) Debug.LogWarning("⚠️ PauseMenu no asignado en UIManager");
        if (hud == null) Debug.LogWarning("⚠️ HUD no asignado en UIManager");

        // Ocultar todos los menús al inicio
        if (missionMenu != null) missionMenu.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (hud != null) hud.SetActive(true);

        Debug.Log("🔄 UI inicializada - Menús ocultos, HUD visible");
    }

    private void SetupButtonListeners()
    {
        // Mission Menu buttons
        if (closeMissionButton != null)
        {
            closeMissionButton.onClick.AddListener(CloseMissionMenu);
            Debug.Log("✅ CloseMissionButton configurado");
        }
        else
        {
            Debug.LogWarning("⚠️ CloseMissionButton no asignado");
        }

        if (acceptMissionButton != null)
        {
            acceptMissionButton.onClick.AddListener(AcceptCurrentMission);
            Debug.Log("✅ AcceptMissionButton configurado");
        }
        else
        {
            Debug.LogWarning("⚠️ AcceptMissionButton no asignado");
        }

        // Pause Menu buttons
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
            Debug.Log("✅ ResumeButton configurado");
        }
        else
        {
            Debug.LogWarning("⚠️ ResumeButton no asignado");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
            Debug.Log("✅ QuitButton configurado");
        }
        else
        {
            Debug.LogWarning("⚠️ QuitButton no asignado");
        }
    }

    private void SetupInputActions()
    {
        try
        {
            // Menú de misiones - M o Tab
            if (enableMissionMenu)
            {
                missionMenuAction = new InputAction("MissionMenu", InputActionType.Button);
                missionMenuAction.AddBinding("<Keyboard>/m");
                missionMenuAction.AddBinding("<Keyboard>/tab");
                missionMenuAction.performed += OnMissionMenuInput;
                missionMenuAction.Enable();
                Debug.Log("🎮 Mission Menu Input configurado (M/Tab)");
            }

            // Menú de pausa - Escape
            if (enablePauseMenu)
            {
                pauseMenuAction = new InputAction("PauseMenu", InputActionType.Button);
                pauseMenuAction.AddBinding("<Keyboard>/escape");
                pauseMenuAction.performed += OnPauseMenuInput;
                pauseMenuAction.Enable();
                Debug.Log("🎮 Pause Menu Input configurado (Escape)");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error configurando Input Actions: {e.Message}");
        }
    }

    #region Input Handlers
    private void OnMissionMenuInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!isInitialized) return;
        if (!enableMissionMenu) return;

        // Verificar que no estamos en pausa
        if (isPauseMenuOpen)
        {
            Debug.Log("⚠️ No se puede abrir Mission Menu mientras está en pausa");
            return;
        }

        ToggleMissionMenu();
    }

    private void OnPauseMenuInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!isInitialized) return;
        if (!enablePauseMenu) return;

        TogglePauseMenu();
    }
    #endregion

    #region Mission Menu
    public void OpenMissionMenu()
    {
        if (!isInitialized) return;
        if (isPauseMenuOpen) return;
        if (missionMenu == null) return;
        if (!enableMissionMenu) return;

        missionMenu.SetActive(true);
        if (hud != null) hud.SetActive(false);
        isMissionMenuOpen = true;

        // Cambiar estado del juego
        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager != null && gameManager.IsReady())
        {
            gameManager.ChangeGameState(GameState.InMenu);
        }

        UpdateMissionDisplay();

        Debug.Log("📋 Menú de misiones abierto");
    }

    public void CloseMissionMenu()
    {
        if (!isInitialized) return;
        if (missionMenu == null) return;

        missionMenu.SetActive(false);
        if (hud != null) hud.SetActive(true);
        isMissionMenuOpen = false;

        // Restaurar estado del juego
        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager != null && gameManager.IsReady())
        {
            gameManager.ChangeGameState(GameState.Exploring);
        }

        Debug.Log("📋 Menú de misiones cerrado");
    }

    private void ToggleMissionMenu()
    {
        if (isMissionMenuOpen)
            CloseMissionMenu();
        else
            OpenMissionMenu();
    }

    private void UpdateMissionDisplay()
    {
        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager == null || !gameManager.IsReady())
        {
            SetMissionDisplayText("GameManager no disponible", "Intenta más tarde", "");
            return;
        }

        if (gameManager.availableMissions.Count > 0)
        {
            Mission currentMission = gameManager.availableMissions[0];
            SetMissionDisplayText(
                currentMission.missionName,
                currentMission.description,
                $"Recompensa: ${currentMission.moneyReward}"
            );
        }
        else
        {
            SetMissionDisplayText(
                "No hay misiones disponibles",
                "Vuelve más tarde para nuevas misiones",
                ""
            );
        }
    }

    private void SetMissionDisplayText(string title, string description, string reward)
    {
        if (missionTitleText != null) missionTitleText.text = title;
        if (missionDescriptionText != null) missionDescriptionText.text = description;
        if (missionRewardText != null) missionRewardText.text = reward;
    }

    private void AcceptCurrentMission()
    {
        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager == null || !gameManager.IsReady()) return;

        if (gameManager.availableMissions.Count > 0)
        {
            Mission mission = gameManager.availableMissions[0];
            gameManager.AcceptMission(mission);
            CloseMissionMenu();

            // Actualizar HUD inmediatamente
            UpdateHUD();
        }
        else
        {
            Debug.LogWarning("⚠️ No hay misiones disponibles para aceptar");
        }
    }
    #endregion

    #region Pause Menu
    public void OpenPauseMenu()
    {
        if (!isInitialized)
        {
            Debug.LogError("❌ UIManager no inicializado en OpenPauseMenu");
            return;
        }

        if (pauseMenu == null)
        {
            Debug.LogError("❌ PauseMenu no asignado en UIManager");
            return;
        }

        if (!enablePauseMenu)
        {
            Debug.LogWarning("⚠️ PauseMenu está deshabilitado en configuración");
            return;
        }

        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager == null)
        {
            Debug.LogError("❌ GameManager no disponible en OpenPauseMenu");
            return;
        }

        if (!gameManager.IsReady())
        {
            Debug.LogError("❌ GameManager no está listo en OpenPauseMenu");
            return;
        }

        // Cerrar mission menu si está abierto
        if (isMissionMenuOpen)
        {
            CloseMissionMenu();
        }

        pauseMenu.SetActive(true);
        if (hud != null) hud.SetActive(false);
        isPauseMenuOpen = true;

        gameManager.ChangeGameState(GameState.Paused);

        Debug.Log("⏸️ Menú de pausa abierto correctamente");
    }

    public void ClosePauseMenu()
    {
        if (!isInitialized) return;
        if (pauseMenu == null) return;

        pauseMenu.SetActive(false);
        if (hud != null) hud.SetActive(true);
        isPauseMenuOpen = false;

        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager != null && gameManager.IsReady())
        {
            gameManager.ChangeGameState(GameState.Exploring);
        }

        Debug.Log("▶️ Menú de pausa cerrado correctamente");
    }

    private void TogglePauseMenu()
    {
        if (isPauseMenuOpen)
            ClosePauseMenu();
        else
            OpenPauseMenu();
    }

    private void ResumeGame()
    {
        ClosePauseMenu();
    }

    private void QuitGame()
    {
        Debug.Log("🚪 Saliendo del juego...");

        // Asegurar que el juego se reanude antes de salir
        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager != null && gameManager.IsReady())
        {
            gameManager.ChangeGameState(GameState.Exploring);
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    #region HUD System
    public void UpdateHUD()
    {
        if (!isInitialized) return;

        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager == null || !gameManager.IsReady()) return;

        // Actualizar contador de misiones
        if (missionCounterText != null)
        {
            missionCounterText.text = $"Misiones: {gameManager.activeMissions.Count}/3";
        }

        // Actualizar dinero
        if (moneyText != null)
        {
            moneyText.text = $"Dinero: ${gameManager.totalEarnings}";
        }

        // Actualizar salud si existe
        UpdateHealthDisplay();
    }

    private void UpdateHealthDisplay()
    {
        if (healthText == null) return;

        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager == null || !gameManager.IsReady()) return;
        if (gameManager.player == null) return;

        PlayerHealth health = gameManager.player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            healthText.text = $"Salud: {health.currentHealth}/{health.maxHealth}";
        }
        else
        {
            healthText.text = "Salud: N/A";
        }
    }

    public void ShowHUD()
    {
        if (hud != null)
        {
            hud.SetActive(true);
            Debug.Log("👁️ HUD mostrado");
        }
    }

    public void HideHUD()
    {
        if (hud != null)
        {
            hud.SetActive(false);
            Debug.Log("👁️ HUD ocultado");
        }
    }
    #endregion

    #region Public Methods
    public void ShowMissionComplete(string missionName, int reward)
    {
        // Aquí podrías implementar un popup de misión completada
        Debug.Log($"🎉 Misión completada: {missionName} - Recompensa: ${reward}");

        // Actualizar HUD inmediatamente
        UpdateHUD();
    }

    public void ShowNotification(string message, float duration = 3f)
    {
        // Aquí podrías implementar un sistema de notificaciones
        Debug.Log($"💬 Notificación: {message}");

        StartCoroutine(ClearNotificationAfter(duration));
    }

    private IEnumerator ClearNotificationAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        // Limpiar notificación
    }

    public bool IsAnyMenuOpen()
    {
        return isMissionMenuOpen || isPauseMenuOpen;
    }

    public void ForceCloseAllMenus()
    {
        if (isMissionMenuOpen) CloseMissionMenu();
        if (isPauseMenuOpen) ClosePauseMenu();
        ShowHUD();

        Debug.Log("🔄 Todos los menús cerrados forzosamente");
    }
    #endregion

    #region Debug Methods
    [ContextMenu("Test Mission Menu")]
    public void TestMissionMenu()
    {
        if (!isInitialized) return;

        if (isMissionMenuOpen)
            CloseMissionMenu();
        else
            OpenMissionMenu();
    }

    [ContextMenu("Test Pause Menu")]
    public void TestPauseMenu()
    {
        if (!isInitialized) return;

        if (isPauseMenuOpen)
            ClosePauseMenu();
        else
            OpenPauseMenu();
    }

    [ContextMenu("Print UI Status")]
    public void PrintUIStatus()
    {
        Debug.Log($"🎮 ESTADO DE UI:");
        Debug.Log($"📋 Mission Menu: {(isMissionMenuOpen ? "Abierto" : "Cerrado")}");
        Debug.Log($"⏸️ Pause Menu: {(isPauseMenuOpen ? "Abierto" : "Cerrado")}");
        Debug.Log($"👁️ HUD: {(hud != null && hud.activeSelf ? "Visible" : "Oculto")}");
        Debug.Log($"🔄 Inicializado: {isInitialized}");
    }
    #endregion

    private void OnDestroy()
    {
        // Limpiar recursos de Input System
        missionMenuAction?.Dispose();
        pauseMenuAction?.Dispose();

        Debug.Log("🗑️ UIManager destruido y recursos liberados");
    }

    // Método público para verificar si está inicializado
    public bool IsReady()
    {
        return isInitialized;
    }
}