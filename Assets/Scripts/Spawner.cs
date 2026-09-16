using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Enemies")]
    public GameObject[] unhealthyEnemies;
    public GameObject[] healthyEnemies;

    [Header("Heart Pickups")]
    public GameObject[] heartPickups;
    [Range(0f, 1f)] public float heartChance = 0.005f;

    [Header("Coin Pickups")]
    public GameObject[] coinPickups;
    [Range(0f, 1f)] public float coinChance = 0.005f;

    [Header("Spawn Area")]
    public Vector2 spawnAreaSize = new Vector2(5f, 5f);
    public bool constrainToCamera = true;
    public float screenPadding = 0.25f;

    [Header("Spawn Rate (auto-ramps)")]
    public float startInterval = 2f;
    public float halveEverySeconds = 20f;
    public float minInterval = 0.25f;

    [Header("Spawn Mix")]
    [Range(0f, 1f)] public float healthyChance = 0.05f;

    private Coroutine loop;
    private Camera gameplayCamera;
    private PlayerMovement player;
    private bool warnedAboutMissingPrefabs;

    private void OnEnable()
    {
        loop = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        // Wait for the scene's player and UI to finish initializing before the first spawn.
        yield return null;
        gameplayCamera = Camera.main;
        player = FindAnyObjectByType<PlayerMovement>();
        float startTime = Time.time;

        while (player == null || !player.IsRoundOver)
        {
            if (Time.timeScale <= 0f)
            {
                yield return null;
                continue;
            }

            SpawnEnemy();
            float elapsed = Time.time - startTime;
            float currentInterval = Mathf.Max(0.01f, startInterval) * Mathf.Pow(
                0.5f, elapsed / Mathf.Max(0.01f, halveEverySeconds));
            currentInterval = Mathf.Max(Mathf.Max(0.01f, minInterval), currentInterval);
            yield return new WaitForSeconds(currentInterval);
        }

        loop = null;
    }

    private void SpawnEnemy()
    {
        GameObject prefabToSpawn = ChoosePrefab();
        if (prefabToSpawn == null)
        {
            if (!warnedAboutMissingPrefabs)
            {
                Debug.LogWarning("Spawner: Assign at least one food or pickup prefab.", this);
                warnedAboutMissingPrefabs = true;
            }

            return;
        }

        warnedAboutMissingPrefabs = false;
        float halfWidth = 0f;
        float halfHeight = 0f;
        SpriteRenderer sprite = prefabToSpawn.GetComponent<SpriteRenderer>();
        if (sprite != null && sprite.sprite != null)
        {
            halfWidth = sprite.sprite.bounds.extents.x * Mathf.Abs(prefabToSpawn.transform.localScale.x);
            halfHeight = sprite.sprite.bounds.extents.y * Mathf.Abs(prefabToSpawn.transform.localScale.y);
        }

        float left = transform.position.x - Mathf.Abs(spawnAreaSize.x) * 0.5f;
        float right = transform.position.x + Mathf.Abs(spawnAreaSize.x) * 0.5f;
        float bottom = transform.position.y - Mathf.Abs(spawnAreaSize.y) * 0.5f;
        float top = transform.position.y + Mathf.Abs(spawnAreaSize.y) * 0.5f;

        if (constrainToCamera && gameplayCamera != null && gameplayCamera.orthographic)
        {
            float cameraHalfWidth = gameplayCamera.orthographicSize * gameplayCamera.aspect;
            float padding = Mathf.Max(0f, screenPadding);
            float visibleLeft = gameplayCamera.transform.position.x - cameraHalfWidth;
            float visibleRight = gameplayCamera.transform.position.x + cameraHalfWidth;
            if (player != null)
            {
                visibleLeft = Mathf.Max(visibleLeft, Mathf.Min(player.minX, player.maxX));
                visibleRight = Mathf.Min(visibleRight, Mathf.Max(player.minX, player.maxX));
            }

            float center = (visibleLeft + visibleRight) * 0.5f;
            visibleLeft = Mathf.Min(visibleLeft + halfWidth + padding, center);
            visibleRight = Mathf.Max(visibleRight - halfWidth - padding, center);
            left = Mathf.Clamp(left, visibleLeft, visibleRight);
            right = Mathf.Clamp(right, left, visibleRight);
            bottom = Mathf.Max(bottom,
                gameplayCamera.transform.position.y + gameplayCamera.orthographicSize + halfHeight + padding);
            top = Mathf.Max(top, bottom);
        }

        Vector3 spawnPosition = new Vector3(Random.Range(left, right), Random.Range(bottom, top), transform.position.z);
        GameObject spawned = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        FallingFoodMotion motion = spawned.GetComponent<FallingFoodMotion>();
        if (motion == null)
        {
            motion = spawned.AddComponent<FallingFoodMotion>();
        }

        motion.Configure(1f + Time.timeSinceLevelLoad / 180f);
    }

    private GameObject ChoosePrefab()
    {
        // One roll gives each pickup its configured chance instead of reducing later pools' odds.
        float heartWeight = HasPrefab(heartPickups) ? Mathf.Clamp01(heartChance) : 0f;
        float coinWeight = HasPrefab(coinPickups) ? Mathf.Clamp01(coinChance) : 0f;
        float healthyWeight = HasPrefab(healthyEnemies) ? Mathf.Clamp01(healthyChance) : 0f;
        float roll = Random.value * Mathf.Max(1f, heartWeight + coinWeight + healthyWeight);
        if (roll < heartWeight)
        {
            return PickPrefab(heartPickups);
        }

        if (roll < heartWeight + coinWeight)
        {
            return PickPrefab(coinPickups);
        }

        if (roll < heartWeight + coinWeight + healthyWeight)
        {
            return PickPrefab(healthyEnemies);
        }

        return PickPrefab(unhealthyEnemies) ?? PickPrefab(healthyEnemies) ??
            PickPrefab(heartPickups) ?? PickPrefab(coinPickups);
    }

    private static bool HasPrefab(GameObject[] prefabs)
    {
        if (prefabs == null)
        {
            return false;
        }

        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
            {
                return true;
            }
        }

        return false;
    }

    private static GameObject PickPrefab(GameObject[] prefabs)
    {
        if (prefabs == null)
        {
            return null;
        }

        GameObject selected = null;
        int count = 0;
        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null && Random.Range(0, ++count) == 0)
            {
                selected = prefab;
            }
        }

        return selected;
    }
}
