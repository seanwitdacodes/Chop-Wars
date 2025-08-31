using System.Collections;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Enemies")]
    public GameObject[] unhealthyEnemies;   // Common enemies
    public GameObject[] healthyEnemies;     // Rare enemies

    [Header("Spawn Area")]
    public Vector2 spawnAreaSize = new Vector2(5f, 5f); // Width & height around spawner

    [Header("Spawn Rate (auto-ramps)")]
    public float startInterval = 2f;        // Starting delay between spawns (seconds)
    public float halveEverySeconds = 30f;   // How often the interval halves (difficulty ramp)

    [Header("Spawn Mix")]
    [Range(0f, 1f)] public float healthyChance = 0.05f; // e.g., 5% rare drop

    private Coroutine loop;

    void OnEnable()
    {
        loop = StartCoroutine(SpawnLoop());
    }

    void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);
    }

    IEnumerator SpawnLoop()
    {
        float startTime = Time.time;

        while (true)
        {
            SpawnEnemy();

            // Exponential decay with NO lower bound:
            // interval(t) = startInterval * 0.5^(t / halveEverySeconds)
            float elapsed = Time.time - startTime;
            float currentInterval = startInterval * Mathf.Pow(
                0.5f,
                elapsed / Mathf.Max(0.0001f, halveEverySeconds)
            );

            // NOTE: With no cap, this will eventually get extremely small and can approach spawning every frame.
            // If you ever need a safety floor, reintroduce: currentInterval = Mathf.Max(verySmallValue, currentInterval);

            yield return new WaitForSeconds(currentInterval);
        }
    }

    void SpawnEnemy()
    {
        if ((unhealthyEnemies == null || unhealthyEnemies.Length == 0) &&
            (healthyEnemies == null || healthyEnemies.Length == 0))
        {
            Debug.LogWarning("FoodSpawner: No enemy prefabs assigned.");
            return;
        }

        // Decide which pool to use (rare vs common)
        GameObject prefabToSpawn;
        if (healthyEnemies != null && healthyEnemies.Length > 0 && Random.value < healthyChance)
        {
            prefabToSpawn = healthyEnemies[Random.Range(0, healthyEnemies.Length)];
        }
        else
        {
            GameObject[] pool = (unhealthyEnemies != null && unhealthyEnemies.Length > 0)
                ? unhealthyEnemies
                : healthyEnemies; // fallback if only healthy is set
            prefabToSpawn = pool[Random.Range(0, pool.Length)];
        }

        // Random position within a box centered on this spawner
        Vector3 spawnPos = new Vector3(
            transform.position.x + Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f),
            transform.position.y + Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f),
            transform.position.z
        );

        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }
}
