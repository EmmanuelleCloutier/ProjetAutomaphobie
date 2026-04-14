using UnityEngine;
using UnityEngine.SceneManagement;

public class MannequinBehavior : MonoBehaviour
{
    public bool bCanMove = false;

    [Tooltip("Distance teleported toward the player each interval (metres).")]
    public float teleportDistance = 0.5f;

    [Tooltip("Time in seconds between each teleport step.")]
    public float teleportInterval = 1f;

    [Header("Jumpscare")]
    [Tooltip("Le GameObject jumpscare à afficher devant le visage du joueur.")]
    public GameObject jumpscareObject;
    [Tooltip("Distance devant la caméra où apparaît le jumpscare.")]
    public float jumpscareDistance = 0.3f;
    [Tooltip("Secondes avant le rechargement du niveau.")]
    public float reloadDelay = 2f;
    public Transform jumpscarePosition;


    private Transform m_PlayerTransform;
    private Camera m_PlayerCamera;
    private float m_Timer = 0f;
    private bool m_IsDead = false;
    private Rigidbody m_Rigidbody;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            m_PlayerTransform = player.transform;
            m_PlayerCamera = player.GetComponentInChildren<Camera>();
        }
        else
        {
            Debug.LogWarning("MannequinBehavior: No GameObject with tag 'Player' found.", this);
        }

        if (jumpscareObject != null)
            jumpscareObject.SetActive(false);
    }

    private void Update()
    {
        if (!bCanMove || m_PlayerTransform == null)
            return;

        m_Timer += Time.deltaTime;
        if (m_Timer >= teleportInterval)
        {
            m_Timer = 0f;

            Vector3 direction = (m_PlayerTransform.position - transform.position).normalized;
            Vector3 newPosition = transform.position + direction * teleportDistance;
            newPosition.y = transform.position.y;
            m_Rigidbody.MovePosition(newPosition);
        }
    }

    /// <summary>
    /// Call this when a raycast from the player first hits this mannequin.
    /// </summary>
    /// <param name="other">The GameObject whose raycast entered.</param>
    public void OnRaycastEnter(GameObject other)
    {
        Debug.Log("Raycast Enter");
        Debug.Log("Cannot move");
        bCanMove = false;
    }

    /// <summary>
    /// Call this when a raycast from the player stops hitting this mannequin.
    /// </summary>
    /// <param name="other">The GameObject whose raycast exited.</param>
    public void OnRaycastExit(GameObject other)
    {
        Debug.Log("Raycast Exit");
        Debug.Log("Can move");
        bCanMove = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Cherche le tag sur l'objet OU ses parents
        if ((other.CompareTag("Player") ||
             other.transform.root.CompareTag("Player")) && !m_IsDead)
        {
            Debug.Log("Collision !!!!");
            TriggerJumpscare();
        }
    }

    private void TriggerJumpscare()
    {
        m_IsDead = true;
        bCanMove = false;

        // Positionner le jumpscare devant la caméra du joueur
        if (jumpscareObject != null && m_PlayerCamera != null)
        {
            jumpscareObject.transform.position = jumpscarePosition.position;
            jumpscareObject.transform.rotation = jumpscarePosition.rotation;
            jumpscareObject.SetActive(true);
        }

        Invoke(nameof(ReloadLevel), reloadDelay);
    }

    private void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
