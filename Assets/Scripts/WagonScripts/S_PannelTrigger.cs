using UnityEngine;

/// <summary>
/// Attach to a button/trigger collider in the wagon.
/// When a GameObject tagged "Key" enters the trigger, the next wagon prefab is spawned
/// at the NextWagon empty object's location, then the metro door slides open.
/// The door is animated via its Animator (trigger "Open"), or slides along a local axis as a fallback.
/// </summary>
[RequireComponent(typeof(Collider))]
public class S_PannelTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The door GameObject to open. Must have an Animator with an 'Open' trigger, or will slide as fallback.")]
    public GameObject door;

    [Header("Wagon Spawning")]
    [Tooltip("The wagon prefab to instantiate when the door is opened.")]
    public GameObject wagonPrefab;

    [Tooltip("Empty GameObject named 'NextWagon' that defines where the next wagon spawns (position & rotation).")]
    public Transform nextWagonSpawnPoint;

    [Header("Key Settings")]
    [Tooltip("Tag used to identify the Key prefab.")]
    public string keyTag = "Key";

    [Tooltip("Destroy the key after it is used to open the door.")]
    public bool consumeKey = true;

    [Header("Fallback Slide (no Animator)")]
    [Tooltip("Local-space axis along which the door slides open (normalized automatically).")]
    public Vector3 slideDirection = new Vector3(1f, 0f, 0f);

    [Tooltip("Distance in units the door slides to fully open.")]
    public float slideDistance = 2f;

    [Tooltip("Duration in seconds for the sliding animation.")]
    public float openDuration = 1f;

    // ?? State ??????????????????????????????????????????????????????????????????
    private bool m_IsOpen = false;
    private bool m_IsAnimating = false;

    // Fallback slide data
    private Vector3 m_ClosedPosition;
    private Vector3 m_OpenPosition;

    // ?? Unity Messages ?????????????????????????????????????????????????????????

    void Start()
    {
        // Make sure the trigger flag is set so OnTriggerEnter fires.
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        if (door != null)
        {
            m_ClosedPosition = door.transform.localPosition;
            m_OpenPosition   = m_ClosedPosition + slideDirection.normalized * slideDistance;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (m_IsOpen || m_IsAnimating) return;

        // Accept the key itself or any child collider belonging to a key root.
        GameObject root = FindKeyRoot(other.gameObject);
        if (root == null) return;

        SpawnNextWagon();
        OpenDoor();

        if (consumeKey)
            Destroy(root);
    }

    // ?? Private Helpers ????????????????????????????????????????????????????????

    /// <summary>
    /// Instantiates the wagon prefab at the NextWagon spawn point's position and rotation.
    /// </summary>
    private void SpawnNextWagon()
    {
        if (wagonPrefab == null)
        {
            Debug.LogWarning($"[S_OpenDoor] No wagon prefab assigned on {gameObject.name}.");
            return;
        }

        if (nextWagonSpawnPoint == null)
        {
            Debug.LogWarning($"[S_OpenDoor] No NextWagon spawn point assigned on {gameObject.name}.");
            return;
        }

        GameObject newWagon = Instantiate(wagonPrefab, nextWagonSpawnPoint.position, nextWagonSpawnPoint.rotation);

        // Tell every S_OpenDoor on the newly spawned wagon that it was created at
        // runtime, so their Start() will auto-open the door.
        // This assignment happens before Unity calls Start() on the new instance.
        foreach (S_OpenDoor openDoor in newWagon.GetComponentsInChildren<S_OpenDoor>(includeInactive: true))
            openDoor.openOnStart = true;

        Debug.Log($"[S_OpenDoor] Wagon '{wagonPrefab.name}' spawned at '{nextWagonSpawnPoint.name}'.");
    }

    /// <summary>
    /// Walks up the hierarchy from <paramref name="obj"/> looking for a GameObject
    /// whose tag matches <see cref="keyTag"/>. Returns null when not found.
    /// </summary>
    private GameObject FindKeyRoot(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.CompareTag(keyTag))
                return current.gameObject;
            current = current.parent;
        }
        return null;
    }

    /// <summary>Opens the door via Animator trigger, or by sliding as a fallback.</summary>
    private void OpenDoor()
    {
        if (door == null)
        {
            Debug.LogWarning($"[S_OpenDoor] No door assigned on {gameObject.name}.");
            return;
        }

        m_IsOpen = true;

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

        Debug.Log($"[S_OpenDoor] Door '{door.name}' opened by key.");
    }

    private System.Collections.IEnumerator SlideDoor()
    {
        m_IsAnimating = true;
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
        m_IsAnimating = false;
    }
}
