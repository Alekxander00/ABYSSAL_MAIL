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

    // Input Actions
    private InputAction missionMenuAction;
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
        // Oficina de correos - M o Tab
        missionMenuAction = new InputAction("MissionMenu", InputActionType.Button);
        missionMenuAction.AddBinding("<Keyboard>/m");
        missionMenuAction.AddBinding("<Keyboard>/tab");
        missionMenuAction.performed += _ => ToggleMissionMenu();
        missionMenuAction.Enable();

        // Menú de pausa - Escape
        pauseMenuAction = new InputAction("PauseMenu", InputActionType.Button);
        pauseMenuAction.AddBinding("<Keyboard>/escape");
        pauseMenuAction.performed += _ => TogglePauseMenu();
        pauseMenuAction.Enable();
    }

    #region Mission Menu - Oficina de Correos
    public void OpenMissionMenu()
    {
        if (isPauseMenuOpen) return;
        if (missionMenu == null) return;

        missionMenu.SetActive(true);
        if (hud != null) hud.SetActive(false);
        isMissionMenuOpen = true;

        GameManager.Instance.ChangeGameState(GameState.InMenu);
        UpdateMissionMenuDisplay();

        Debug.Log("🏣 Oficina de correos abierta");
    }

    public void CloseMissionMenu()
    {
        if (missionMenu == null) return;

        missionMenu.SetActive(false);
        if (hud != null) hud.SetActive(true);
        isMissionMenuOpen = false;

        GameManager.Instance.ChangeGameState(GameState.Exploring);

        Debug.Log("🏣 Oficina de correos cerrada");
    }

    private void ToggleMissionMenu()
    {
        if (isMissionMenuOpen)
            CloseMissionMenu();
        else
            OpenMissionMenu();
    }

    private void UpdateMissionMenuDisplay()
    {
        ClearMissionButtons();

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.IsReady())
        {
            SetMissionDisplayText("Oficina de Correos", "No hay misiones disponibles", "Vuelve más tarde", "");
            return;
        }

        // Listar todas las misiones disponibles
        if (gameManager.availableMissions.Count > 0)
        {
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
        }
    }

    private void CreateMissionButton(Mission mission)
    {
        if (missionButtonPrefab == null || missionsContainer == null) return;

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
    }

    private void ClearMissionButtons()
    {
        if (missionsContainer == null) return;

        foreach (Transform child in missionsContainer)
        {
            Destroy(child.gameObject);
        }
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

            acceptMissionButton.interactable = GameManager.Instance.CanAcceptMission(mission);
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
            GameManager.Instance.AcceptMission(selectedMission);
            CloseMissionMenu();
            UpdateHUD();
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
    }

    public void ClosePauseMenu()
    {
        if (pauseMenu == null) return;

        pauseMenu.SetActive(false);
        if (hud != null) hud.SetActive(true);
        isPauseMenuOpen = false;

        GameManager.Instance.ChangeGameState(GameState.Exploring);
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
        // Ocultar el popup
    }
    #endregion

    private void OnDestroy()
    {
        missionMenuAction?.Dispose();
        pauseMenuAction?.Dispose();
    }

    public bool IsReady()
    {
        return isInitialized;
    }
}