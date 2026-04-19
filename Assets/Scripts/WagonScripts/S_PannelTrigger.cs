using UnityEngine;

/// <summary>
/// Attach to a button/trigger collider in the wagon.
/// When a GameObject tagged "Key" enters the trigger, the next wagon prefab is spawned
/// at the NextWagon empty object's location, then the metro door slides open.
/// The door is animated via its Animator (trigger "Open"), or rotates around an axis as a fallback.
/// </summary>
[RequireComponent(typeof(Collider))]
public class S_PannelTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The door GameObject to open. Must have an Animator with an 'Open' trigger, or will rotate as fallback.")]
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

    [Header("Fallback Rotation (no Animator)")]
    [Tooltip("Local-space axis around which the door rotates open.")]
    public Vector3 rotationAxis = new Vector3(0f, 1f, 0f);

    [Tooltip("Degrees the door rotates to fully open (positive = counterclockwise around axis).")]
    public float rotationAngle = 90f;

    [Tooltip("Duration in seconds for the rotation animation.")]
    public float openDuration = 1f;

    // ?? State ??????????????????????????????????????????????????????????????????
    private bool m_IsOpen = false;
    private bool m_IsAnimating = false;

    // Fallback rotation data
    private Quaternion m_ClosedRotation;
    private Quaternion m_OpenRotation;

    // ?? Unity Messages ?????????????????????????????????????????????????????????

    void Start()
    {
        // Make sure the trigger flag is set so OnTriggerEnter fires.
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        if (door != null)
        {
            m_ClosedRotation = door.transform.localRotation;
            m_OpenRotation   = m_ClosedRotation * Quaternion.AngleAxis(rotationAngle, rotationAxis.normalized);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[S_PannelTrigger] OnTriggerEnter hit by '{other.gameObject.name}' (tag: '{other.tag}')");

        if (m_IsOpen || m_IsAnimating)
        {
            Debug.Log($"[S_PannelTrigger] Ignored – m_IsOpen={m_IsOpen}, m_IsAnimating={m_IsAnimating}");
            return;
        }

        GameObject root = FindKeyRoot(other.gameObject);
        if (root == null)
        {
            Debug.Log($"[S_PannelTrigger] No key root found on '{other.gameObject.name}' – expected tag '{keyTag}'");
            return;
        }

        Debug.Log($"[S_PannelTrigger] Key '{root.name}' accepted – spawning wagon and opening door.");

        S_SpawnOnBench.WagonNumber++;

        SpawnNextWagon();
        OpenDoor();

        if (consumeKey)
            Destroy(root);
    }

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

        // The root of the wagon this panel belongs to – will be destroyed once
        // the new wagon's door closes behind the player.
        GameObject currentWagonRoot = transform.root.gameObject;

        // Tell every S_OpenDoor on the newly spawned wagon that it was created at
        // runtime, so their Start() will auto-open the door.
        // This assignment happens before Unity calls Start() on the new instance.
        foreach (S_OpenDoor openDoor in newWagon.GetComponentsInChildren<S_OpenDoor>(includeInactive: true))
            openDoor.openOnStart = true;

        // Wire up every S_CloseDoor on the new wagon so it knows which wagon to
        // destroy once the player has crossed over and the door has shut.
        foreach (S_CloseDoor closeDoor in newWagon.GetComponentsInChildren<S_CloseDoor>(includeInactive: true))
            closeDoor.previousWagon = currentWagonRoot;

        Debug.Log($"[S_OpenDoor] Wagon '{wagonPrefab.name}' spawned at '{nextWagonSpawnPoint.name}'. " +
                  $"Previous wagon '{currentWagonRoot.name}' queued for destruction on door close.");
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

    /// <summary>Opens the door via Animator trigger, or by rotating as a fallback.</summary>
    private void OpenDoor()
    {
        if (door == null)
        {
            Debug.LogWarning($"[S_PannelTrigger] No door assigned on {gameObject.name}.");
            return;
        }

        m_IsOpen = true;

        Animator animator = door.GetComponent<Animator>();
        if (animator != null)
        {
            Debug.Log($"[S_PannelTrigger] Using Animator on '{door.name}' – setting trigger 'Open'.");
            animator.SetTrigger("Open");
        }
        else
        {
            Debug.Log($"[S_PannelTrigger] No Animator on '{door.name}' – starting rotation coroutine. ClosedRot={m_ClosedRotation.eulerAngles}, OpenRot={m_OpenRotation.eulerAngles}");
            StartCoroutine(RotateDoor());
        }
    }

    private System.Collections.IEnumerator RotateDoor()
    {
        m_IsAnimating = true;
        float elapsed = 0f;

        Debug.Log($"[S_PannelTrigger] RotateDoor coroutine started. Door: '{door.name}', ClosedRot={m_ClosedRotation.eulerAngles}, OpenRot={m_OpenRotation.eulerAngles}");

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            t = t * t * (3f - 2f * t);
            door.transform.localRotation = Quaternion.Slerp(m_ClosedRotation, m_OpenRotation, t);
            Debug.Log($"[S_PannelTrigger] RotateDoor t={t:F2}, localRot={door.transform.localRotation.eulerAngles}, elapsed={elapsed:F2}/{openDuration}");
            yield return null;
        }

        door.transform.localRotation = m_OpenRotation;
        Debug.Log($"[S_PannelTrigger] RotateDoor complete. Final localRot={door.transform.localRotation.eulerAngles}");
        m_IsAnimating = false;
    }
}
