using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Enemies")]
    public GameObject[] unhealthyEnemies;   // Common enemies
    public GameObject[] healthyEnemies;     // Rare enemies

    [Header("Special Pickups")]
    public GameObject[] heartPickups;       // Heart prefabs
    [Range(0f, 1f)] public float heartChance = 0.005f; // 0.5% chance by default

    [Header("Spawn Area")]
    public Vector2 spawnAreaSize = new Vector2(5f, 5f);

    [Header("Spawn Rate (auto-ramps)")]
    public float startInterval = 2f;
    public float halveEverySeconds = 30f;

    [Header("Spawn Mix")]
    [Range(0f, 1f)] public float healthyChance = 0.05f; // 5% chance

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

            float elapsed = Time.time - startTime;
            float currentInterval = startInterval * Mathf.Pow(
                0.5f,
                elapsed / Mathf.Max(0.0001f, halveEverySeconds)
            );

            yield return new WaitForSeconds(currentInterval);
        }
    }

    void SpawnEnemy()
    {
        if ((unhealthyEnemies == null || unhealthyEnemies.Length == 0) &&
            (healthyEnemies == null || healthyEnemies.Length == 0) &&
            (heartPickups == null || heartPickups.Length == 0))
        {
            Debug.LogWarning("Spawner: No prefabs assigned.");
            return;
        }

        GameObject prefabToSpawn = null;

        // First: roll for heart
        if (heartPickups != null && heartPickups.Length > 0 && Random.value < heartChance)
        {
            prefabToSpawn = heartPickups[Random.Range(0, heartPickups.Length)];
        }
        // Second: roll for healthy food
        else if (healthyEnemies != null && healthyEnemies.Length > 0 && Random.value < healthyChance)
        {
            prefabToSpawn = healthyEnemies[Random.Range(0, healthyEnemies.Length)];
        }
        // Otherwise: unhealthy food
        else
        {
            GameObject[] pool = (unhealthyEnemies != null && unhealthyEnemies.Length > 0)
                ? unhealthyEnemies
                : healthyEnemies;
            prefabToSpawn = pool[Random.Range(0, pool.Length)];
        }

        // Random spawn position
        Vector3 spawnPos = new Vector3(
            transform.position.x + Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f),
            transform.position.y + Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f),
            transform.position.z
        );

        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }
}
