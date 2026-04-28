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
    public GameObject[] sittingEnemyPrefabs;
    public GameObject[] standingEnemyPrefabs;
    public GameObject[] sittingDummyPrefabs;
    public GameObject[] standingDummyPrefabs;
    public GameObject keyPrefab;

    [Header("Bench & Pole Discovery")]
    [Tooltip("Name that bench children must contain to be considered valid candidates.")]
    public string benchNameFilter = "Bench";

    [Tooltip("Name that pole children must contain to be considered valid candidates.")]
    public string poleNameFilter = "SM_pole";

    [Tooltip("Name of the child Transform inside each bench used for sitting entity/key spawning.")]
    public string spawnPointName = "SpawnPoint";

    [Tooltip("Name of the child Transform inside each pole used for standing entity spawning.")]
    public string enemySpawnPointName = "EnemySpawn";

    [Header("Timing")]
    public bool spawnOnStart = true;
    public float spawnDelay = 0f;

    // ── Internal Helper Data ──────────────────────────────────────────────────

    private struct SpawnSlot
    {
        public Transform transform;
        public bool isBench;
    }

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

        // Shuffle initial lists to ensure randomness 
        Shuffle(availableBenches);
        Shuffle(availablePoles);

        // 1. Spawn key first on an available bench, so it is strictly placed on a bench
        if (keyPrefab != null && availableBenches.Count > 0)
        {
            Transform bench = availableBenches[0];
            availableBenches.RemoveAt(0); // Take this bench slot out of the pool
            
            Transform spawnTransform = bench.Find(spawnPointName) ?? bench;
            GameObject spawnedKey = Instantiate(keyPrefab, spawnTransform.position, spawnTransform.rotation);
            s_CurrentSpawned.Add(spawnedKey);
            Debug.Log($"[S_SpawnOnBench] Key spawned on '{bench.name}'.");
        }

        // 2. Pool remaining slots together
        List<SpawnSlot> allSlots = new List<SpawnSlot>();
        foreach (var b in availableBenches) allSlots.Add(new SpawnSlot { transform = b, isBench = true });
        foreach (var p in availablePoles)   allSlots.Add(new SpawnSlot { transform = p, isBench = false });

        Shuffle(allSlots); // Shuffle benches and poles together to perfectly randomize entities placement

        // 3. Deal out enemies and dummies
        int totalEntities = EnemyCount + DummyCount;
        for (int i = 0; i < totalEntities; i++)
        {
            if (i >= allSlots.Count)
            {
                Debug.LogWarning($"[S_SpawnOnBench] Ran out of slots (needed {totalEntities}, have {allSlots.Count}).");
                break;
            }

            bool isEnemy = i < EnemyCount;
            SpawnSlot slot = allSlots[i];
            
            // Choose the correct array based on entity type and slot type (sitting/bench vs standing/pole)
            GameObject[] prefabArray;
            if (isEnemy)
                prefabArray = slot.isBench ? sittingEnemyPrefabs : standingEnemyPrefabs;
            else
                prefabArray = slot.isBench ? sittingDummyPrefabs : standingDummyPrefabs;

            if (prefabArray == null || prefabArray.Length == 0)
            {
                Debug.LogWarning($"[S_SpawnOnBench] Missing prefabs for {(isEnemy ? "Enemy" : "Dummy")} on {(slot.isBench ? "Bench" : "Pole")} – skipping.");
                continue;
            }

            GameObject prefabToSpawn = prefabArray[Random.Range(0, prefabArray.Length)];
            if (prefabToSpawn == null) continue;

            string targetPointName = slot.isBench ? spawnPointName : enemySpawnPointName;
            Transform targetTransform = slot.transform.Find(targetPointName) ?? slot.transform;

            GameObject spawnedEntity = Instantiate(prefabToSpawn, targetTransform.position, targetTransform.rotation);
            s_CurrentSpawned.Add(spawnedEntity);

            Debug.Log($"[S_SpawnOnBench] {(isEnemy ? "Enemy" : "Dummy")} '{prefabToSpawn.name}' spawned on '{slot.transform.name}'.");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
