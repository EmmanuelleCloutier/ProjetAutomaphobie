using UnityEngine;

public class S_OpenDoor : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The door GameObject to open. Must have an Animator with an 'Open' trigger, or will slide as fallback.")]
    public GameObject door;

    [Header("Fallback Slide (no Animator)")]
    [Tooltip("Local-space axis along which the door slides open (normalized automatically).")]
    public Vector3 slideDirection = new Vector3(1f, 0f, 0f);

    [Tooltip("Distance in units the door slides to fully open.")]
    public float slideDistance = 2f;

    [Tooltip("Duration in seconds for the sliding animation.")]
    public float openDuration = 1f;

    /// <summary>
    /// Set this to <c>true</c> immediately after <c>Instantiate</c> so that
    /// <c>Start</c> automatically opens the door.
    /// Objects already present in the scene at startup never receive this call,
    /// so their door remains closed.
    /// </summary>
    [HideInInspector]
    public bool openOnStart = false;

    // Fallback slide data
    private Vector3 m_ClosedPosition;
    private Vector3 m_OpenPosition;

    private void Start()
    {
        if (door != null)
        {
            m_ClosedPosition = door.transform.localPosition;
            m_OpenPosition   = m_ClosedPosition + slideDirection.normalized * slideDistance;
        }

        if (openOnStart)
            OpenDoor();
    }

    public void OpenDoor()
    {
        if (door == null)
        {
            Debug.LogWarning($"[S_OpenDoor] No door assigned on {gameObject.name}.");
            return;
        }

        Animator animator = door.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Open");
        }
        else
        {
            // Fallback: smoothly slide the door.
            StartCoroutine(SlideDoor());
        }

        Debug.Log($"[S_OpenDoor] Door '{door.name}' opened on spawn.");
    }

    private System.Collections.IEnumerator SlideDoor()
    {
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            // Smooth-step easing
            t = t * t * (3f - 2f * t);
            door.transform.localPosition = Vector3.Lerp(m_ClosedPosition, m_OpenPosition, t);
            yield return null;
        }

        door.transform.localPosition = m_OpenPosition;
    }
}
