using UnityEngine;

public class S_OpenDoor : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The door GameObject to open. Must have an Animator with an 'Open' trigger, or will rotate as fallback.")]
    public GameObject door;

    [Header("Fallback Rotation (no Animator)")]
    [Tooltip("Local-space axis around which the door rotates open.")]
    public Vector3 rotationAxis = new Vector3(0f, 1f, 0f);

    [Tooltip("Degrees the door rotates to fully open (positive = counterclockwise around axis).")]
    public float rotationAngle = 90f;

    [Tooltip("Duration in seconds for the rotation animation.")]
    public float openDuration = 1f;

    /// <summary>
    /// Set this to <c>true</c> immediately after <c>Instantiate</c> so that
    /// <c>Start</c> automatically opens the door.
    /// Objects already present in the scene at startup never receive this call,
    /// so their door remains closed.
    /// </summary>
    [HideInInspector]
    public bool openOnStart = false;

    // Fallback rotation data
    private Quaternion m_ClosedRotation;
    private Quaternion m_OpenRotation;

    private void Start()
    {
        if (door != null)
        {
            m_ClosedRotation = door.transform.localRotation;
            m_OpenRotation   = m_ClosedRotation * Quaternion.AngleAxis(rotationAngle, rotationAxis.normalized);
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
            // Fallback: smoothly rotate the door.
            StartCoroutine(RotateDoor());
        }

        Debug.Log($"[S_OpenDoor] Door '{door.name}' opened on spawn.");
    }

    private System.Collections.IEnumerator RotateDoor()
    {
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            // Smooth-step easing
            t = t * t * (3f - 2f * t);
            door.transform.localRotation = Quaternion.Slerp(m_ClosedRotation, m_OpenRotation, t);
            yield return null;
        }

        door.transform.localRotation = m_OpenRotation;
    }
}
