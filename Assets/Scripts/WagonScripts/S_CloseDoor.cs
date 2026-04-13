using UnityEngine;

/// <summary>
/// Attach to a box-volume trigger collider inside the wagon.
/// When a GameObject tagged <see cref="playerTag"/> enters the trigger,
/// the door is closed via its Animator ("Close" trigger) or slides back
/// to its closed position as a fallback.
/// Once the closing animation is complete the <see cref="previousWagon"/>
/// GameObject is destroyed.
/// </summary>
[RequireComponent(typeof(Collider))]
public class S_CloseDoor : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The door GameObject to close. Must have an Animator with a 'Close' trigger, or will slide as fallback.")]
    public GameObject door;

    [Tooltip("The previous wagon GameObject to destroy once the door has finished closing.")]
    public GameObject previousWagon;

    [Header("Player Settings")]
    [Tooltip("Tag used to identify the player.")]
    public string playerTag = "Player";

    [Header("Fallback Slide (no Animator)")]
    [Tooltip("Local-space axis along which the door slides open (normalized automatically). The door will slide back along the opposite direction to close.")]
    public Vector3 slideDirection = new Vector3(0f, 0f, 1f);

    [Tooltip("Distance in units the door slides when fully open (used to compute the open position).")]
    public float slideDistance = 1f;

    [Tooltip("Duration in seconds for the closing animation.")]
    public float closeDuration = 1f;

    [Header("Animator Path")]
    [Tooltip("When an Animator is used, the previous wagon is destroyed after this delay (seconds). Set it to match the length of the 'Close' animation clip.")]
    public float animatorDestroyDelay = 1f;

    // ── State ────────────────────────────────────────────────────────────────
    private bool m_IsClosed = false;
    private bool m_IsAnimating = false;

    // Fallback slide data
    private Vector3 m_ClosedPosition;
    private Vector3 m_OpenPosition;

    // ── Unity Messages ───────────────────────────────────────────────────────

    private void Start()
    {
        // Ensure the collider is a trigger so OnTriggerEnter fires.
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        if (door != null)
        {
            // The door starts in its open position; record both positions for the slide fallback.
            m_OpenPosition   = door.transform.localPosition;
            m_ClosedPosition = m_OpenPosition - slideDirection.normalized * slideDistance;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_IsClosed || m_IsAnimating) return;

        if (!other.CompareTag(playerTag)) return;

        CloseDoor();
    }

    // ── Private Helpers ──────────────────────────────────────────────────────

    /// <summary>Closes the door via Animator trigger, or by sliding as a fallback.</summary>
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
            // Fallback: smoothly slide the door back to its closed position,
            // then destroy the previous wagon at the end of the coroutine.
            StartCoroutine(SlideDoorClosed());
        }

        Debug.Log($"[S_CloseDoor] Door '{door.name}' closed – player entered trigger on '{gameObject.name}'.");
    }

    private void DestroyPreviousWagon()
    {
        if (previousWagon == null) return;

        Debug.Log($"[S_CloseDoor] Destroying previous wagon '{previousWagon.name}'.");
        Destroy(previousWagon);
    }

    private System.Collections.IEnumerator SlideDoorClosed()
    {
        m_IsAnimating = true;
        float elapsed = 0f;
        Vector3 startPosition = door.transform.localPosition;

        while (elapsed < closeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration);
            // Smooth-step easing
            t = t * t * (3f - 2f * t);
            door.transform.localPosition = Vector3.Lerp(startPosition, m_ClosedPosition, t);
            yield return null;
        }

        door.transform.localPosition = m_ClosedPosition;
        m_IsAnimating = false;

        DestroyPreviousWagon();
    }
}
