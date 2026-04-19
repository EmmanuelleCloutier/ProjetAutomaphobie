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

    [Header("Son")]
    [Tooltip("JumpScareSFX")]
    public AudioClip jumpscareSound;
    [Tooltip("Volume")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Tooltip("StepSFX")]
    public AudioClip footstepSounds;
    [Tooltip("Volume")]
    [Range(0f, 1f)]
    public float footstepVolume = 1f;
    public AudioSource m_AudioSource;


    private Transform m_PlayerTransform;
    private Camera m_PlayerCamera;
    private float m_Timer = 0f;
    private bool m_IsDead = false;
    private Rigidbody m_Rigidbody;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();

        if (m_AudioSource == null)
        {
            GameObject audioManager = GameObject.Find("AudioManager");
            if (audioManager != null)
                m_AudioSource = audioManager.GetComponent<AudioSource>();
            else
                Debug.LogWarning("MannequinBehavior: No GameObject named 'AudioManager' found in the scene.", this);
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            m_PlayerTransform = player.transform;
            m_PlayerCamera = player.GetComponentInChildren<Camera>();
            jumpscarePosition = GameObject.FindWithTag("Jumpscare").transform;
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

            if (footstepSounds != null && m_AudioSource != null)
            {
                AudioClip clip = footstepSounds;
                if (clip != null)
                    m_AudioSource.PlayOneShot(clip, footstepVolume);
            }

            newPosition.y = transform.position.y;
            transform.position = newPosition;
            //m_Rigidbody.MovePosition(newPosition);
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
        Debug.Log("Trigger Enter: " + other.gameObject.name);
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

        // Spawn a new jumpscare instance oriented in front of the player camera
        if (jumpscareObject != null && m_PlayerCamera != null)
        {
            Vector3 spawnPosition = m_PlayerCamera.transform.position + m_PlayerCamera.transform.forward * jumpscareDistance;
            Quaternion spawnRotation = m_PlayerCamera.transform.rotation;
            GameObject instance = Instantiate(jumpscareObject, spawnPosition, spawnRotation);
            instance.SetActive(true);
        }

        if (jumpscareSound != null && m_AudioSource != null)
        {
            m_AudioSource.PlayOneShot(jumpscareSound, soundVolume);
        }

        Invoke(nameof(ReloadLevel), reloadDelay);
    }

    private void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
