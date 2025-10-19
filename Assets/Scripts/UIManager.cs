using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Menu References")]
    public GameObject missionMenu; // Oficina de correos
    public GameObject pauseMenu;
    public GameObject hud;

    [Header("Mission Menu References - Oficina de Correos")]
    public Transform missionsContainer; // Contenedor para listar misiones
    public TextMeshProUGUI missionTitleText;
    public TextMeshProUGUI missionDescriptionText;
    public TextMeshProUGUI missionRewardText;
    public TextMeshProUGUI missionNPCText;
    public Button acceptMissionButton;
    public Button closeMissionButton;

    [Header("Mission Prefab")]
    public GameObject missionButtonPrefab; // Prefab para botones de misión

    [Header("Pause Menu References")]
    public Button resumeButton;
    public Button quitButton;

    [Header("HUD References")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI missionCounterText;
    public TextMeshProUGUI moneyText;

    private bool isMissionMenuOpen = false;
    private bool isPauseMenuOpen = false;
    private bool isInitialized = false;

    // Input Actions - SOLO PAUSA
    private InputAction pauseMenuAction;

    // Misión seleccionada actualmente
    private Mission selectedMission;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        SetupInputActions();

        GameManager.GetOrCreateInstance().ForceInitialize();

        isInitialized = true;
        Debug.Log("✅ UIManager completamente inicializado - Teclas M/Tab desactivadas");
    }

    private void Update()
    {
        if (isInitialized)
        {
            UpdateHUD();
        }
    }

    private void InitializeUI()
    {
        if (missionMenu != null) missionMenu.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (hud != null) hud.SetActive(true);
    }

    private void SetupButtonListeners()
    {
        if (closeMissionButton != null)
            closeMissionButton.onClick.AddListener(CloseMissionMenu);

        if (acceptMissionButton != null)
            acceptMissionButton.onClick.AddListener(AcceptSelectedMission);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void SetupInputActions()
    {
        // ✅ SOLO configurar menú de pausa - ELIMINADO menú de misiones por teclado
        pauseMenuAction = new InputAction("PauseMenu", InputActionType.Button);
        pauseMenuAction.AddBinding("<Keyboard>/escape");
        pauseMenuAction.performed += _ => TogglePauseMenu();
        pauseMenuAction.Enable();

        Debug.Log("🎮 Input System configurado - Solo Escape para pausa");
    }

    #region Mission Menu - Oficina de Correos (SOLO DESDE INTERACCIÓN)
    public void OpenMissionMenu()
    {
        Debug.Log("🎯 UIManager.OpenMissionMenu() ejecutándose desde Oficina...");

        if (isPauseMenuOpen)
        {
            Debug.Log("⏸ No se puede abrir menú de misiones - Menú de pausa abierto");
            return;
        }

        if (missionMenu == null)
        {
            Debug.LogError("❌ missionMenu es null - No asignado en el Inspector");
            return;
        }

        Debug.Log($"✅ missionMenu encontrado - Estado actual: {missionMenu.activeSelf}");

        missionMenu.SetActive(true);
        Debug.Log("✅ missionMenu activado");

        if (hud != null)
        {
            hud.SetActive(false);
            Debug.Log("✅ HUD desactivado");
        }
        else
        {
            Debug.LogWarning("⚠️ hud es null");
        }

        isMissionMenuOpen = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.InMenu);
            Debug.Log("✅ Estado del juego cambiado a InMenu");
        }
        else
        {
            Debug.LogError("❌ GameManager.Instance es null");
        }

        UpdateMissionMenuDisplay();
        Debug.Log("✅ Menú de misiones completamente abierto desde Oficina de Correos");
    }

    public void CloseMissionMenu()
    {
        if (missionMenu == null) return;

        missionMenu.SetActive(false);
        if (hud != null) hud.SetActive(true);
        isMissionMenuOpen = false;

        GameManager.Instance.ChangeGameState(GameState.Exploring);

        // ✅ NOTIFICAR a la oficina que el menú se cerró
        NotifyPostOfficeMenuClosed();

        Debug.Log("🏣 Menú de misiones cerrado");
    }

    // ✅ NUEVO: Notificar a la oficina que el menú se cerró
    private void NotifyPostOfficeMenuClosed()
    {
        // Buscar la oficina de correos en la escena
        PostOfficeInteractable postOffice = FindObjectOfType<PostOfficeInteractable>();
        if (postOffice != null)
        {
            postOffice.OnMissionMenuClosed();
            Debug.Log("📞 Notificando a Oficina de Correos que el menú se cerró");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró Oficina de Correos para notificar cierre del menú");
        }
    }

    private void UpdateMissionMenuDisplay()
    {
        ClearMissionButtons();

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.IsReady())
        {
            SetMissionDisplayText("Oficina de Correos", "Sistema no disponible", "Vuelve más tarde", "");
            Debug.LogWarning("⚠️ GameManager no disponible para mostrar misiones");
            return;
        }

        // Listar todas las misiones disponibles
        if (gameManager.availableMissions.Count > 0)
        {
            Debug.Log($"📋 Mostrando {gameManager.availableMissions.Count} misiones disponibles");

            foreach (Mission mission in gameManager.availableMissions)
            {
                CreateMissionButton(mission);
            }

            // Seleccionar la primera misión por defecto
            SelectMission(gameManager.availableMissions[0]);
        }
        else
        {
            SetMissionDisplayText("Oficina de Correos", "No hay misiones disponibles", "Vuelve más tarde para nuevas entregas", "");
            acceptMissionButton.interactable = false;
            Debug.Log("📭 No hay misiones disponibles para mostrar");
        }
    }

    private void CreateMissionButton(Mission mission)
    {
        if (missionButtonPrefab == null || missionsContainer == null)
        {
            Debug.LogError("❌ missionButtonPrefab o missionsContainer no asignados");
            return;
        }

        GameObject missionButtonObj = Instantiate(missionButtonPrefab, missionsContainer);
        Button missionButton = missionButtonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = missionButtonObj.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText != null)
        {
            buttonText.text = $"{mission.missionName} - ${mission.moneyReward}";
        }

        if (missionButton != null)
        {
            missionButton.onClick.AddListener(() => SelectMission(mission));
        }

        Debug.Log($"✅ Botón de misión creado: {mission.missionName}");
    }

    private void ClearMissionButtons()
    {
        if (missionsContainer == null) return;

        foreach (Transform child in missionsContainer)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("🧹 Botones de misiones limpiados");
    }

    private void SelectMission(Mission mission)
    {
        selectedMission = mission;

        if (mission != null)
        {
            SetMissionDisplayText(
                mission.missionName,
                mission.description,
                $"Recompensa: ${mission.moneyReward}",
                $"Entregar a: {mission.targetNPCName}"
            );

            bool canAccept = GameManager.Instance.CanAcceptMission(mission);
            acceptMissionButton.interactable = canAccept;

            Debug.Log($"🎯 Misión seleccionada: {mission.missionName} - Puede aceptar: {canAccept}");
        }
    }

    private void SetMissionDisplayText(string title, string description, string reward, string npc)
    {
        if (missionTitleText != null) missionTitleText.text = title;
        if (missionDescriptionText != null) missionDescriptionText.text = description;
        if (missionRewardText != null) missionRewardText.text = reward;
        if (missionNPCText != null) missionNPCText.text = npc;
    }

    private void AcceptSelectedMission()
    {
        if (selectedMission != null && GameManager.Instance != null)
        {
            Debug.Log($"✅ Aceptando misión: {selectedMission.missionName}");
            GameManager.Instance.AcceptMission(selectedMission);
            CloseMissionMenu();
            UpdateHUD();
        }
        else
        {
            Debug.LogError("❌ No se puede aceptar misión - selectedMission o GameManager es null");
        }
    }
    #endregion

    #region Pause Menu
    public void OpenPauseMenu()
    {
        if (pauseMenu == null) return;

        pauseMenu.SetActive(true);
        if (hud != null) hud.SetActive(false);
        isPauseMenuOpen = true;

        GameManager.Instance.ChangeGameState(GameState.Paused);
        Debug.Log("⏸ Menú de pausa abierto");
    }

    public void ClosePauseMenu()
    {
        if (pauseMenu == null) return;

        pauseMenu.SetActive(false);
        if (hud != null) hud.SetActive(true);
        isPauseMenuOpen = false;

        GameManager.Instance.ChangeGameState(GameState.Exploring);
        Debug.Log("⏸ Menú de pausa cerrado");
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
        GameManager.Instance.ChangeGameState(GameState.Exploring);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    #endregion

    #region HUD Updates
    public void UpdateHUD()
    {
        if (!isInitialized) return;

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.IsReady()) return;

        // Actualizar contador de misiones
        if (missionCounterText != null)
        {
            missionCounterText.text = $"Entregas: {gameManager.activeMissions.Count}/3";
        }

        // Actualizar dinero
        if (moneyText != null)
        {
            moneyText.text = $"Dinero: ${gameManager.totalEarnings}";
        }
    }

    public void ShowMissionComplete(string missionName, int reward)
    {
        StartCoroutine(ShowMissionCompleteCoroutine(missionName, reward));
    }

    private IEnumerator ShowMissionCompleteCoroutine(string missionName, int reward)
    {
        // Aquí podrías mostrar un popup de misión completada
        Debug.Log($"🎉 Misión completada: {missionName} - Recompensa: ${reward}");

        // Actualizar HUD
        UpdateHUD();

        yield return new WaitForSeconds(2f);
        // Ocultar el popup (implementar si tienes un popup visual)
    }
    #endregion

    #region Public Methods para otros sistemas
    public bool IsMissionMenuOpen()
    {
        return isMissionMenuOpen;
    }

    public bool IsPauseMenuOpen()
    {
        return isPauseMenuOpen;
    }

    public void ForceCloseAllMenus()
    {
        CloseMissionMenu();
        ClosePauseMenu();
        Debug.Log("🔄 Todos los menús forzados a cerrar");
    }
    #endregion

    private void OnDestroy()
    {
        // ✅ SOLO dispose de pause action - ELIMINADO missionMenuAction
        pauseMenuAction?.Dispose();

        Debug.Log("♻️ UIManager destruido - Recursos liberados");
    }

    public bool IsReady()
    {
        return isInitialized;
    }
}