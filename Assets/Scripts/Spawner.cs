using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Enemies")]
    public GameObject[] unhealthyEnemies;
    public GameObject[] healthyEnemies;

    [Header("Heart Pickups")]
    public GameObject[] heartPickups;
    [Range(0f, 1f)] public float heartChance = 0.012f;

    [Header("Coin Pickups")]
    public GameObject[] coinPickups;
    [Range(0f, 1f)] public float coinChance = 0.02f;

    [Header("Spawn Area")]
    public Vector2 spawnAreaSize = new Vector2(5f, 5f);
    public bool constrainToCamera = true;
    public float screenPadding = 0.25f;

    [Header("Spawn Rate (auto-ramps)")]
    public float startInterval = 1.6f;
    [Tooltip("Time to halve the remaining gap between the starting and fastest spawn interval.")]
    public float halveEverySeconds = 55f;
    public float minInterval = 0.32f;

    [Header("Falling Difficulty")]
    public float fallRampSeconds = 120f;
    [Range(1f, 3f)] public float maxFallMultiplier = 3f;

    [Header("Spawn Mix")]
    [Tooltip("Bonus chances at the starting pace; adjusted to stay rare as hazards speed up.")]
    [Range(0f, 1f)] public float healthyChance = 0.04f;

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

        while (player == null || !player.IsRoundOver)
        {
            if (Time.timeScale <= 0f)
            {
                yield return null;
                continue;
            }

            // Scaled scene time freezes during pauses and resets only for a new run.
            float elapsed = Time.timeSinceLevelLoad;
            SpawnEnemy(elapsed);
            yield return new WaitForSeconds(GetSpawnInterval(elapsed));
        }

        loop = null;
    }

    public float GetSpawnInterval(float elapsed)
    {
        float fastest = Mathf.Max(0.05f, minInterval);
        float starting = Mathf.Max(fastest, startInterval);
        return fastest + (starting - fastest) * Mathf.Pow(
            0.5f, Mathf.Max(0f, elapsed) / Mathf.Max(0.01f, halveEverySeconds));
    }

    public float GetFallMultiplier(float elapsed)
    {
        return Mathf.Lerp(1f, Mathf.Clamp(maxFallMultiplier, 1f, 3f),
            1f - Mathf.Pow(0.5f, Mathf.Max(0f, elapsed) / Mathf.Max(0.01f, fallRampSeconds)));
    }

    private void SpawnEnemy(float elapsed)
    {
        GameObject prefabToSpawn = ChoosePrefab(elapsed);
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

        motion.Configure(GetFallMultiplier(elapsed));
    }

    private GameObject ChoosePrefab(float elapsed)
    {
        // Preserve bonuses per minute instead of flooding late-game runs with rewards.
        float bonusScale = GetSpawnInterval(elapsed) / GetSpawnInterval(0f);
        float heartWeight = HasPrefab(heartPickups) ? Mathf.Clamp01(heartChance) * bonusScale : 0f;
        float coinWeight = HasPrefab(coinPickups) ? Mathf.Clamp01(coinChance) * bonusScale : 0f;
        float healthyWeight = HasPrefab(healthyEnemies) ? Mathf.Clamp01(healthyChance) * bonusScale : 0f;
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
