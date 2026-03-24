using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform topSpawn;
    public Transform bottomSpawn;
    public Transform leftSpawn;
    public Transform rightSpawn;

    [Header("Bubble Prefab")]
    public GameObject bubblePrefab;

    [Header("Plane")]
    public float fixedY = 0.5f;

    [Header("Spawn Timing")]
    public float startSpawnInterval = 1.8f;
    public float minimumSpawnInterval = 0.55f;
    public float difficultyRamp = 0.05f;

    private float spawnTimer;
    private float elapsedTime;

    private void Start()
    {
        spawnTimer = startSpawnInterval;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

        elapsedTime += Time.deltaTime;
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnBubble();

            float currentInterval = Mathf.Max(
                minimumSpawnInterval,
                startSpawnInterval - elapsedTime * difficultyRamp
            );

            spawnTimer = currentInterval;
        }
    }

    private void SpawnBubble()
    {
        if (bubblePrefab == null) return;

        Transform spawnPoint = GetRandomSpawnPoint();
        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.y = fixedY;

        GameObject obj = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);

        Bubble bubble = obj.GetComponent<Bubble>();
        if (bubble != null)
        {
            bubble.fixedY = fixedY;
            bubble.bubblePrefab = bubblePrefab;
            bubble.SetType(GetBubbleTypeForCurrentTime());
        }
    }

    private Transform GetRandomSpawnPoint()
    {
        int rand = Random.Range(0, 4);

        switch (rand)
        {
            case 0: return topSpawn;
            case 1: return bottomSpawn;
            case 2: return leftSpawn;
            default: return rightSpawn;
        }
    }

    private BubbleType GetBubbleTypeForCurrentTime()
    {
        float t = elapsedTime;

        if (t < 20f)
        {
            return BubbleType.Normal;
        }
        else if (t < 40f)
        {
            int r = Random.Range(0, 100);
            if (r < 60) return BubbleType.Normal;
            if (r < 80) return BubbleType.Volatile;
            return BubbleType.Heavy;
        }
        else if (t < 60f)
        {
            int r = Random.Range(0, 100);
            if (r < 40) return BubbleType.Normal;
            if (r < 58) return BubbleType.Volatile;
            if (r < 76) return BubbleType.Heavy;
            return BubbleType.Split;
        }
        else if (t < 90f)
        {
            int r = Random.Range(0, 100);
            if (r < 28) return BubbleType.Normal;
            if (r < 42) return BubbleType.Volatile;
            if (r < 56) return BubbleType.Heavy;
            if (r < 70) return BubbleType.Split;
            if (r < 86) return BubbleType.Implosion;
            return BubbleType.Corrupted;
        }
        else
        {
            int r = Random.Range(0, 100);
            if (r < 20) return BubbleType.Normal;
            if (r < 33) return BubbleType.Volatile;
            if (r < 46) return BubbleType.Heavy;
            if (r < 59) return BubbleType.Split;
            if (r < 72) return BubbleType.Implosion;
            if (r < 86) return BubbleType.Corrupted;
            return BubbleType.Momentum;
        }
    }
}