using UnityEngine;

/// <summary>
/// Attach to a bench (or any GameObject) in the wagon.
/// Spawns the "Key" prefab at the position and rotation defined by an empty
/// GameObject (<see cref="keySpawnPoint"/>) assigned in the Inspector.
/// The spawn can be triggered on Start or called externally via <see cref="SpawnKey"/>.
/// </summary>
public class S_SpawnOnBench : MonoBehaviour
{
    [Header("Key Spawning")]
    [Tooltip("The Key prefab to instantiate on the bench.")]
    public GameObject keyPrefab;

    [Tooltip("Empty GameObject that defines where the key spawns (position & rotation).")]
    public Transform keySpawnPoint;

    [Tooltip("Spawn the key automatically when the scene starts.")]
    public bool spawnOnStart = true;

    [Tooltip("Optional delay in seconds before the key is spawned (only used when Spawn On Start is enabled).")]
    public float spawnDelay = 0f;

    // Prevent spawning more than once.
    private bool m_HasSpawned = false;

    // ?? Unity Messages ?????????????????????????????????????????????????????????

    void Start()
    {
        if (!spawnOnStart) return;

        if (spawnDelay > 0f)
            Invoke(nameof(SpawnKey), spawnDelay);
        else
            SpawnKey();
    }

    // ?? Public API ?????????????????????????????????????????????????????????????

    /// <summary>
    /// Spawns the key prefab at the spawn point.
    /// Safe to call multiple times; only the first call has any effect.
    /// </summary>
    public void SpawnKey()
    {
        if (m_HasSpawned) return;

        if (keyPrefab == null)
        {
            Debug.LogWarning($"[S_SpawnOnBench] No key prefab assigned on {gameObject.name}.");
            return;
        }

        if (keySpawnPoint == null)
        {
            Debug.LogWarning($"[S_SpawnOnBench] No key spawn point assigned on {gameObject.name}.");
            return;
        }

        Instantiate(keyPrefab, keySpawnPoint.position, keySpawnPoint.rotation);
        m_HasSpawned = true;

        Debug.Log($"[S_SpawnOnBench] Key '{keyPrefab.name}' spawned at '{keySpawnPoint.name}'.");
    }
}
