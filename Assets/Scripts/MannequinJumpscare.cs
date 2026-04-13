using UnityEngine;
using System.Collections;

public class MannequinJumpscare : MonoBehaviour
{
    [Header("Jumpscare Settings")]
    public float triggerDistance = 1.2f;      // Distance pour déclencher
    public float faceDistance = 0.4f;          // À quelle distance du visage il apparaît
    public float jumpscareHoldTime = 1.5f;     // Temps qu'il reste devant le visage
    public float fadeOutTime = 1f;             // Durée du fade to black

    [Header("References")]
    public Transform playerHead;
    public MannequinAI mannequinAI;

    private bool triggered = false;
    private CanvasGroup fadeCanvas;

    void Awake()
    {
        if (playerHead == null)
            playerHead = Camera.main.transform;

        if (mannequinAI == null)
            mannequinAI = GetComponent<MannequinAI>();

        SetupFadeCanvas();
    }

    void Update()
    {
        if (triggered) return;
        if (!mannequinAI.IsActive()) return; // Vérifie que le mannequin est activé

        float distance = Vector3.Distance(transform.position, playerHead.position);

        if (distance <= triggerDistance)
        {
            triggered = true;
            StartCoroutine(DoJumpscare());
        }
    }

    IEnumerator DoJumpscare()
    {
        // 1. Désactive le NavMeshAgent pour pouvoir bouger librement
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // 2. Téléporte le mannequin devant le visage
        Vector3 jumpscarePosition = playerHead.position + playerHead.forward * faceDistance;
        jumpscarePosition.y = playerHead.position.y; // Même hauteur que les yeux
        transform.position = jumpscarePosition;

        // 3. Le fait regarder le joueur
        transform.LookAt(playerHead);
        transform.rotation = Quaternion.LookRotation(playerHead.position - transform.position);

        // 4. Attend pendant jumpscareHoldTime
        yield return new WaitForSeconds(jumpscareHoldTime);

        // 5. Fade to black
        yield return StartCoroutine(FadeOut());

        // 6. Ici tu peux : recharger la scène, afficher un écran de mort, etc.
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    // --- Fade to black ---

    void SetupFadeCanvas()
    {
        // Crée un canvas de fade automatiquement
        GameObject canvasGO = new GameObject("FadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 999;

        // Le colle sur la caméra
        canvasGO.transform.SetParent(playerHead);
        canvasGO.transform.localPosition = new Vector3(0, 0, 0.1f);
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        // Panneau noir
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasGO.transform);
        var image = panel.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.black;

        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        fadeCanvas = canvasGO.AddComponent<CanvasGroup>();
        fadeCanvas.alpha = 0f;
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Clamp01(elapsed / fadeOutTime);
            yield return null;
        }
    }
}