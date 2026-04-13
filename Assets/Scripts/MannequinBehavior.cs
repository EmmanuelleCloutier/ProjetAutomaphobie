using UnityEngine;

public class MannequinBehavior : MonoBehaviour
{
    public bool bCanMove = false;

    [Tooltip("Distance teleported toward the player each interval (metres).")]
    public float teleportDistance = 0.5f;

    [Tooltip("Time in seconds between each teleport step.")]
    public float teleportInterval = 1f;

    private Transform m_PlayerTransform;
    private float m_Timer = 0f;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            m_PlayerTransform = player.transform;
        else
            Debug.LogWarning("MannequinBehavior: No GameObject with tag 'Player' found.", this);
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
            transform.position = newPosition;
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
}
