using UnityEngine;

/// <summary>
/// Attach to a box-volume trigger collider inside the wagon.
/// When a GameObject tagged <see cref="playerTag"/> enters the trigger,
/// the door is closed via its Animator ("Close" trigger) or rotates back
/// to its closed position as a fallback.
/// Once the closing animation is complete the <see cref="previousWagon"/>
/// GameObject is destroyed.
/// </summary>
[RequireComponent(typeof(Collider))]
public class S_CloseDoor : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The door GameObject to close. Must have an Animator with a 'Close' trigger, or will rotate as fallback.")]
    public GameObject door;

    [Tooltip("The previous wagon GameObject to destroy once the door has finished closing.")]
    public GameObject previousWagon;

    [Header("Player Settings")]
    [Tooltip("Tag used to identify the player.")]
    public string playerTag = "Player";

    [Header("Fallback Rotation (no Animator)")]
    [Tooltip("Local-space axis around which the door rotates (same as S_OpenDoor).")]
    public Vector3 rotationAxis = new Vector3(0f, 1f, 0f);

    [Tooltip("Degrees the door was rotated when open (used to compute the open rotation).")]
    public float rotationAngle = 90f;

    [Tooltip("Duration in seconds for the closing animation.")]
    public float closeDuration = 1f;

    [Header("Animator Path")]
    [Tooltip("Delay before destroying the previous wagon when using an Animator (match the 'Close' clip length).")]
    public float animatorDestroyDelay = 1f;

    // ── State ────────────────────────────────────────────────────────────────
    private bool m_IsClosed = false;
    private bool m_IsAnimating = false;

    // Fallback rotation data
    private Quaternion m_ClosedRotation;
    private Quaternion m_OpenRotation;

    // ── Unity Messages ───────────────────────────────────────────────────────

    private void Start()
    {
        // Ensure the collider is a trigger so OnTriggerEnter fires.
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        if (door != null)
        {
            // The door starts in its open rotation; derive the closed rotation from it.
            m_OpenRotation   = door.transform.localRotation;
            m_ClosedRotation = m_OpenRotation * Quaternion.AngleAxis(-rotationAngle, rotationAxis.normalized);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_IsClosed || m_IsAnimating) return;

        if (!other.CompareTag(playerTag)) return;

        CloseDoor();
    }

    // ── Private Helpers ──────────────────────────────────────────────────────

    /// <summary>Closes the door via Animator trigger, or by rotating as a fallback.</summary>
    private void CloseDoor()
    {
        if (door == null)
        {
            Debug.LogWarning($"[S_CloseDoor] No door assigned on {gameObject.name}.");
            return;
        }

        m_IsClosed = true;

        Animator animator = door.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Close");
            // Destroy the previous wagon after the animation clip has had time to play.
            Invoke(nameof(DestroyPreviousWagon), animatorDestroyDelay);
        }
        else
        {
            // Fallback: smoothly rotate the door back to its closed rotation,
            // then destroy the previous wagon at the end of the coroutine.
            StartCoroutine(RotateDoorClosed());
        }

        Debug.Log($"[S_CloseDoor] Door '{door.name}' closed – player entered trigger on '{gameObject.name}'.");
    }

    private void DestroyPreviousWagon()
    {
        // Destroy all bench-spawned objects from the previous wagon before the wagon itself.
        S_SpawnOnBench.DestroyAll();

        if (previousWagon == null) return;

        Debug.Log($"[S_CloseDoor] Destroying previous wagon '{previousWagon.name}'.");
        Destroy(previousWagon);
    }

    private System.Collections.IEnumerator RotateDoorClosed()
    {
        m_IsAnimating = true;
        float elapsed = 0f;
        Quaternion startRotation = door.transform.localRotation;

        while (elapsed < closeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration);
            // Smooth-step easing
            t = t * t * (3f - 2f * t);
            door.transform.localRotation = Quaternion.Slerp(startRotation, m_ClosedRotation, t);
            yield return null;
        }

        door.transform.localRotation = m_ClosedRotation;
        m_IsAnimating = false;

        DestroyPreviousWagon();
    }
}
