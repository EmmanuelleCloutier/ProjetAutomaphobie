using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Simulacres — MenuManager
/// Gère les trois menus (Titre, Pause, Fin) sur un World Space Canvas
/// Compatible Quest 3 / OpenXR / UI Toolkit
///
/// Setup dans Unity :
///   1. Créer un GameObject "MenuPanel" dans la scène
///   2. Ajouter UIDocument (source asset = MenuTitre.uxml / MenuPause.uxml / MenuFin.uxml)
///   3. Ajouter TrackedDeviceGraphicRaycaster sur le même GameObject
///   4. S'assurer que l'EventSystem de scène a un XRUIInputModule
///   5. Attacher ce script au GameObject "MenuPanel"
///   6. Assigner les références dans l'Inspector
/// </summary>
public class MenuManager : MonoBehaviour
{
    // ── Inspector refs ──────────────────────────────────────────────
    [Header("UI Documents")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Panel Assets (UXML)")]
    [SerializeField] private VisualTreeAsset menuTitreAsset;
    [SerializeField] private VisualTreeAsset menuPauseAsset;
    [SerializeField] private VisualTreeAsset menuFinAsset;

    [Header("World Space Settings")]
    [Tooltip("Distance from XR camera where the panel spawns (metres)")]
    [SerializeField] private float panelDistance = 1.8f;

    [Tooltip("Panel world height (metres). Width = height * (1080/1350).")]
    [SerializeField] private float panelHeight = 0.9f;

    [Header("Game References")]
    [SerializeField] private Transform xrCameraTransform;

    // ── State ────────────────────────────────────────────────────────
    public enum MenuState { None, Titre, Pause, Fin }
    private MenuState currentState = MenuState.None;

    // ── UI element caches ────────────────────────────────────────────
    private VisualElement root;

    // Pause
    private VisualElement anxietyFill;
    private Label anxietyPercent;
    private Label wagonValue;

    // Fin
    private Label statWagons;
    private Label statAnxiety;
    private Label statTime;
    private Label rapportApproches;
    private Label rapportFreeze;
    private Label rapportCles;

    // ── Lifecycle ────────────────────────────────────────────────────
    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        PositionPanelInFrontOfCamera();
    }

    private void Start()
    {
        ShowMenuTitre();
    }

    private void Update()
    {
        if (currentState == MenuState.None) return;

        // Quest 3: menu button (primary menu button) toggles pause
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    // ── Public API ───────────────────────────────────────────────────

    public void ShowMenuTitre()
    {
        LoadScreen(menuTitreAsset);
        currentState = MenuState.Titre;

        root.Q<Button>("btn-jouer").clicked += OnJouerClicked;
        root.Q<Button>("btn-parametres").clicked += OnParametresClicked;
        root.Q<Button>("btn-quitter").clicked += OnQuitterClicked;

        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ShowMenuPause(int wagonActuel, int wagonTotal, float anxieteNormalized)
    {
        LoadScreen(menuPauseAsset);
        currentState = MenuState.Pause;

        wagonValue   = root.Q<Label>("wagon-value");
        anxietyFill  = root.Q<VisualElement>("anxiety-bar-fill");
        anxietyPercent = root.Q<Label>("anxiety-value");

        SetPauseData(wagonActuel, wagonTotal, anxieteNormalized);

        root.Q<Button>("btn-reprendre").clicked    += OnReprendreClicked;
        root.Q<Button>("btn-recommencer").clicked  += OnRecommencerClicked;
        root.Q<Button>("btn-menu").clicked         += OnMenuDepuisPauseClicked;

        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ShowMenuFin(int wagons, float anxieteMax, float tempsSecondes,
                            int mannequinsApproches, int mannequinsTotal,
                            int freezeCount, int clesTrouvees, int clesTotal)
    {
        LoadScreen(menuFinAsset);
        currentState = MenuState.Fin;

        root.Q<Label>("stat-wagons-value").text   = wagons.ToString("D2");
        root.Q<Label>("stat-anxiety-value").text  = Mathf.RoundToInt(anxieteMax * 100f) + "%";
        root.Q<Label>("stat-time-value").text     = FormatTime(tempsSecondes);
        root.Q<Label>("r1-value").text            = $"{mannequinsApproches} / {mannequinsTotal}";
        root.Q<Label>("r2-value").text            = freezeCount.ToString();
        root.Q<Label>("r3-value").text            = $"{clesTrouvees} / {clesTotal}";

        root.Q<Button>("btn-rejouer").clicked += OnRejouerClicked;
        root.Q<Button>("btn-menu").clicked    += OnMenuDepuisFinClicked;

        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Met à jour la barre d'anxiété en cours de partie depuis l'extérieur si besoin.
    /// anxieteNormalized : 0.0 → 1.0
    /// </summary>
    public void UpdateAnxiety(float anxieteNormalized)
    {
        if (currentState != MenuState.Pause) return;
        SetAnxietyBar(anxieteNormalized);
    }

    public void TogglePause()
    {
        if (currentState == MenuState.Pause)
            OnReprendreClicked();
        else if (currentState == MenuState.None)
            ShowMenuPause(1, 3, 0.62f); // valeurs placeholder — passer les vraies depuis le GameManager
    }

    // ── Button callbacks ─────────────────────────────────────────────

    private void OnJouerClicked()
    {
        HideMenu();
        // Charger la scène de jeu
        // SceneManager.LoadScene("Wagon_01");
        Debug.Log("[Simulacres] Démarrage du jeu");
    }

    private void OnParametresClicked()
    {
        Debug.Log("[Simulacres] Ouvrir paramètres");
        // TODO : ouvrir un panneau de paramètres séparé
    }

    private void OnQuitterClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnReprendreClicked()
    {
        HideMenu();
        Time.timeScale = 1f;
        currentState = MenuState.None;
    }

    private void OnRecommencerClicked()
    {
        HideMenu();
        Time.timeScale = 1f;
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("[Simulacres] Recommencer le wagon");
    }

    private void OnMenuDepuisPauseClicked()
    {
        HideMenu();
        Time.timeScale = 1f;
        ShowMenuTitre();
    }

    private void OnRejouerClicked()
    {
        HideMenu();
        Time.timeScale = 1f;
        // SceneManager.LoadScene("Wagon_01");
        Debug.Log("[Simulacres] Rejouer depuis le début");
    }

    private void OnMenuDepuisFinClicked()
    {
        HideMenu();
        Time.timeScale = 1f;
        ShowMenuTitre();
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private void LoadScreen(VisualTreeAsset asset)
    {
        uiDocument.visualTreeAsset = asset;
        root = uiDocument.rootVisualElement.Q<VisualElement>("root");
    }

    private void HideMenu()
    {
        gameObject.SetActive(false);
    }

    private void SetPauseData(int wagonActuel, int wagonTotal, float anxieteNormalized)
    {
        if (wagonValue != null)
            wagonValue.text = $"{wagonActuel:D2} / {wagonTotal:D2}";
        SetAnxietyBar(anxieteNormalized);
    }

    private void SetAnxietyBar(float normalized)
    {
        float pct = Mathf.Clamp01(normalized) * 100f;
        if (anxietyFill != null)
            anxietyFill.style.width = Length.Percent(pct);
        if (anxietyPercent != null)
            anxietyPercent.text = Mathf.RoundToInt(pct) + "%";
    }

    private string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:D2}:{s:D2}";
    }

    /// <summary>
    /// Place le panneau UI devant la caméra XR au démarrage.
    /// Le panneau tourne toujours face au joueur grâce au PanelSettings Billboard.
    /// </summary>
    private void PositionPanelInFrontOfCamera()
    {
        if (xrCameraTransform == null)
        {
            var mainCam = Camera.main;
            if (mainCam != null) xrCameraTransform = mainCam.transform;
        }

        if (xrCameraTransform == null) return;

        Vector3 forward = xrCameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        transform.position = xrCameraTransform.position + forward * panelDistance;
        transform.rotation = Quaternion.LookRotation(forward);

        // Scale : 1px = 0.001m (1mm), canvas 1080x1350px → 1.08m x 1.35m
        float scale = panelHeight / 1350f;
        transform.localScale = Vector3.one * scale;
    }
}
