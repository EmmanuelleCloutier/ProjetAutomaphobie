using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to the parent GameObject that contains "Bench" and pole (SM_pole) children.
/// Distributes enemies on poles (EnemySpawn point) and dummies on benches (SpawnPoint),
/// and optionally a key on one of the remaining benches.
/// </summary>
public class S_SpawnOnBench : MonoBehaviour
{
    // ── Static counts ─────────────────────────────────────────────────────────

    public static int WagonNumber = 1;
    public static int EnemyCount = 0;
    public static int DummyCount = 2;

    // ── Static spawn registry ─────────────────────────────────────────────────

    private static readonly List<GameObject> s_PreviousSpawned = new List<GameObject>();
    private static readonly List<GameObject> s_CurrentSpawned  = new List<GameObject>();

    // ── Inspector fields ──────────────────────────────────────────────────────

    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject dummyPrefab;
    public GameObject keyPrefab;

    [Header("Bench Discovery")]
    [Tooltip("Name that bench children must contain to be considered valid dummy spawn candidates.")]
    public string benchNameFilter = "Bench";

    [Tooltip("Name that pole children must contain to be considered valid enemy spawn candidates.")]
    public string poleNameFilter = "SM_pole";

    [Tooltip("Name of the child Transform inside each bench used for dummy/key spawning.")]
    public string spawnPointName = "SpawnPoint";

    [Tooltip("Name of the child Transform inside each pole used for enemy spawning.")]
    public string enemySpawnPointName = "EnemySpawn";

    [Header("Timing")]
    public bool spawnOnStart = true;
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

    public void SpawnAll()
    {
        DummyCount = WagonNumber + 1;
        EnemyCount = DummyCount / 3;

        Debug.Log($"[S_SpawnOnBench] WagonNumber={WagonNumber} → DummyCount={DummyCount}, EnemyCount={EnemyCount}");

        s_PreviousSpawned.Clear();
        s_PreviousSpawned.AddRange(s_CurrentSpawned);
        s_CurrentSpawned.Clear();

        // Collect benches (for dummies/key) and poles (for enemies) separately.
        var availableBenches = new List<Transform>();
        var availablePoles   = new List<Transform>();

        foreach (Transform child in transform)
        {
            if (child.name.Contains(benchNameFilter))
                availableBenches.Add(child);
            else if (child.name.Contains(poleNameFilter))
                availablePoles.Add(child);
        }

        if (availableBenches.Count == 0 && availablePoles.Count == 0)
        {
            Debug.LogWarning($"[S_SpawnOnBench] No bench or pole children found on '{gameObject.name}'.");
            return;
        }

        // Shuffle both lists (Fisher-Yates).
        Shuffle(availableBenches);
        Shuffle(availablePoles);

        // Spawn enemies on poles.
        int poleIndex = 0;
        SpawnGroup(enemyPrefab, "Enemy", EnemyCount, availablePoles, ref poleIndex, enemySpawnPointName);

        // Spawn dummies on benches.
        int benchIndex = 0;
        SpawnGroup(dummyPrefab, "Dummy", DummyCount, availableBenches, ref benchIndex, spawnPointName);

        // Spawn the key on one of the remaining benches.
        if (keyPrefab != null)
            SpawnGroup(keyPrefab, "Key", 1, availableBenches, ref benchIndex, spawnPointName);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void Shuffle(List<Transform> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void SpawnGroup(GameObject prefab, string label, int count,
                            List<Transform> slots, ref int index, string spawnPoint)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[S_SpawnOnBench] '{label}' prefab is not assigned – skipping.");
            index += count;
            return;
        }

        for (int i = 0; i < count; i++, index++)
        {
            if (index >= slots.Count)
            {
                Debug.LogWarning($"[S_SpawnOnBench] Ran out of slots while spawning '{label}' " +
                                 $"(needed slot {index}, have {slots.Count}).");
                return;
            }

            Transform slot = slots[index];
            Transform spawnTransform = slot.Find(spawnPoint) ?? slot;

            GameObject spawned = Instantiate(prefab, spawnTransform.position, spawnTransform.rotation);
            s_CurrentSpawned.Add(spawned);

            Debug.Log($"[S_SpawnOnBench] {label} '{prefab.name}' spawned on '{slot.name}' " +
                      $"at '{spawnTransform.name}'.");
        }
    }
}
