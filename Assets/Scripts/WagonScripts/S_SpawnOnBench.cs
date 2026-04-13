using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to the parent GameObject that contains many "Bench" children.
/// Distributes enemies, dummies, and optionally a key across the
/// bench children so that each bench receives at most one object.
///
/// Counts are stored in static fields so they can be shared / tweaked from
/// other scripts at runtime (e.g. a difficulty manager).
/// </summary>
public class S_SpawnOnBench : MonoBehaviour
{
    // ── Static counts (shared across all instances / configurable at runtime) ─

    /// <summary>Number of enemy prefabs to spawn across the benches.</summary>
    public static int EnemyCount = 2;

    /// <summary>Number of dummy prefabs to spawn across the benches.</summary>
    public static int DummyCount = 3;

    // ── Static spawn registry (shared across ALL instances) ───────────────────

    // Objects spawned by the previous wagon's instance – destroyed when that wagon closes.
    private static readonly List<GameObject> s_PreviousSpawned = new List<GameObject>();

    // Objects spawned by the current (most recent) instance.
    private static readonly List<GameObject> s_CurrentSpawned = new List<GameObject>();

    // ── Inspector fields ──────────────────────────────────────────────────────

    [Header("Prefabs")]
    [Tooltip("The enemy prefab to instantiate on a bench.")]
    public GameObject enemyPrefab;

    [Tooltip("The dummy prefab to instantiate on a bench.")]
    public GameObject dummyPrefab;

    [Tooltip("Optional key prefab to instantiate on one of the remaining benches.")]
    public GameObject keyPrefab;

    [Header("Bench Discovery")]
    [Tooltip("Name that bench children must contain to be considered valid spawn candidates.")]
    public string benchNameFilter = "Bench";

    [Tooltip("Name of the child Transform inside each bench that marks the exact spawn position. " +
             "Falls back to the bench root if not found.")]
    public string spawnPointName = "SpawnPoint";

    [Header("Timing")]
    [Tooltip("Spawn everything automatically when the scene starts.")]
    public bool spawnOnStart = true;

    [Tooltip("Optional delay in seconds before spawning (only used when Spawn On Start is enabled).")]
    public float spawnDelay = 0f;

    // ── Unity Messages ────────────────────────────────────────────────────────

    void Start()
    {
        if (!spawnOnStart) return;

        if (spawnDelay > 0f)
            Invoke(nameof(SpawnAll), spawnDelay);
        else
            SpawnAll();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Destroys every GameObject spawned by the <em>previous</em> instance of this
    /// script. Objects belonging to the current wagon are left untouched.
    /// </summary>
    public static void DestroyAll()
    {
        foreach (GameObject obj in s_PreviousSpawned)
        {
            if (obj != null)
            {
                Debug.Log($"[S_SpawnOnBench] Destroying previously spawned object '{obj.name}'.");
                Destroy(obj);
            }
        }

        s_PreviousSpawned.Clear();
    }

    /// <summary>
    /// Distributes enemies, dummies, and the key across the bench children at
    /// random, ensuring each bench receives at most one object.
    /// Promotes the current spawn list to "previous" before filling the new one,
    /// so <see cref="DestroyAll"/> only ever destroys the old wagon's objects.
    /// </summary>
    public void SpawnAll()
    {
        // Whatever was current is now the previous generation.
        s_PreviousSpawned.Clear();
        s_PreviousSpawned.AddRange(s_CurrentSpawned);
        s_CurrentSpawned.Clear();

        // Collect all direct children whose name contains the bench filter.
        var availableBenches = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.name.Contains(benchNameFilter))
                availableBenches.Add(child);
        }

        if (availableBenches.Count == 0)
        {
            Debug.LogWarning($"[S_SpawnOnBench] No bench children found on '{gameObject.name}' " +
                             $"(filter: '{benchNameFilter}').");
            return;
        }

        int totalNeeded = EnemyCount + DummyCount + (keyPrefab != null ? 1 : 0);
        if (availableBenches.Count < totalNeeded)
        {
            Debug.LogWarning($"[S_SpawnOnBench] Not enough benches ({availableBenches.Count}) " +
                             $"for {totalNeeded} objects on '{gameObject.name}'. " +
                             $"Some objects will not be spawned.");
        }

        // Shuffle the bench list (Fisher-Yates).
        for (int i = availableBenches.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (availableBenches[i], availableBenches[j]) = (availableBenches[j], availableBenches[i]);
        }

        int benchIndex = 0;

        // Spawn enemies.
        SpawnGroup(enemyPrefab, "Enemy", EnemyCount, availableBenches, ref benchIndex);

        // Spawn dummies.
        SpawnGroup(dummyPrefab, "Dummy", DummyCount, availableBenches, ref benchIndex);

        // Spawn the key on one of the remaining benches.
        if (keyPrefab != null)
            SpawnGroup(keyPrefab, "Key", 1, availableBenches, ref benchIndex);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Spawns <paramref name="count"/> instances of <paramref name="prefab"/> on
    /// consecutive entries in <paramref name="benches"/>, starting at
    /// <paramref name="index"/> (passed by reference so the caller advances it).
    /// Each instance is registered in the shared static list so it can be destroyed
    /// by any future call to <see cref="DestroyAll"/>.
    /// </summary>
    private void SpawnGroup(GameObject prefab, string label, int count,
                            List<Transform> benches, ref int index)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[S_SpawnOnBench] '{label}' prefab is not assigned – skipping.");
            index += count; // Still advance so later groups stay in the right slot.
            return;
        }

        for (int i = 0; i < count; i++, index++)
        {
            if (index >= benches.Count)
            {
                Debug.LogWarning($"[S_SpawnOnBench] Ran out of benches while spawning '{label}' " +
                                 $"(needed slot {index}, have {benches.Count}).");
                return;
            }

            Transform bench = benches[index];
            Transform spawnPoint = bench.Find(spawnPointName) ?? bench;

            GameObject spawned = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            s_CurrentSpawned.Add(spawned);

            Debug.Log($"[S_SpawnOnBench] {label} '{prefab.name}' spawned on bench " +
                      $"'{bench.name}' at '{spawnPoint.name}'.");
        }
    }
}
