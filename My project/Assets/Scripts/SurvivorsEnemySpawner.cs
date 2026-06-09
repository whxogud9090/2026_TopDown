using UnityEngine;

public class SurvivorsEnemySpawner : MonoBehaviour
{
    public EnemyController enemyPrefab;
    public Transform player;
    public float spawnInterval = 1.2f;
    public float minSpawnInterval = 0.18f;
    public float spawnDistance = 11f;
    public int maxEnemies = 80;
    public int maxEnemiesLimit = 220;
    public float difficultyStepTime = 25f;
    public int extraEnemyPerStep = 1;
    public float eliteStartTime = 75f;
    public float crowdRampStartTime = 60f;
    public float waveStartTime = 60f;
    public float waveInterval = 120f;
    public int baseWaveSpawnCount = 8;

    private float nextSpawnTime;
    private float nextWaveTime;
    private int waveNumber;

    private void Start()
    {
        nextWaveTime = waveStartTime;
    }

    private void Update()
    {
        if (enemyPrefab == null || player == null || Time.time < nextSpawnTime)
            return;

        TrySpawnWave();

        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies)
            return;

        var difficulty = GetDifficultyLevel();
        var crowdRamp = GetCrowdRampLevel();
        var currentInterval = Mathf.Max(minSpawnInterval, spawnInterval - difficulty * 0.055f - crowdRamp * 0.04f);
        nextSpawnTime = Time.time + currentInterval;
        maxEnemies = Mathf.Min(maxEnemiesLimit, 70 + difficulty * 8 + crowdRamp * 10);

        var spawnCount = Mathf.Min(6, 1 + difficulty / 3 + crowdRamp);
        for (var i = 0; i < spawnCount; i++)
            SpawnEnemy(difficulty);
    }

    private void TrySpawnWave()
    {
        if (Time.timeSinceLevelLoad < nextWaveTime)
            return;

        var currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (currentEnemyCount >= maxEnemies)
        {
            nextWaveTime = Time.timeSinceLevelLoad + 8f;
            return;
        }

        waveNumber++;
        var difficulty = GetDifficultyLevel();
        var burstCount = Mathf.Min(22, baseWaveSpawnCount + waveNumber * 2);
        burstCount = Mathf.Min(burstCount, maxEnemies - currentEnemyCount);

        for (var i = 0; i < burstCount; i++)
            SpawnEnemy(difficulty);

        FloatingText.Spawn(player.position + Vector3.up * 1.45f, "WAVE " + waveNumber, new Color(1f, 0.35f, 0.15f, 1f));
        CameraShake.Shake(0.08f, 0.1f);
        nextWaveTime = Time.timeSinceLevelLoad + waveInterval;
    }

    private int GetDifficultyLevel()
    {
        return Mathf.FloorToInt(Time.timeSinceLevelLoad / difficultyStepTime);
    }

    private int GetCrowdRampLevel()
    {
        if (Time.timeSinceLevelLoad < crowdRampStartTime)
            return 0;

        return 1 + Mathf.FloorToInt((Time.timeSinceLevelLoad - crowdRampStartTime) / 45f);
    }

    private void SpawnEnemy(int difficulty)
    {
        var direction = Random.insideUnitCircle.normalized;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        var randomOffset = Random.Range(-1.6f, 1.6f);
        var position = (Vector2)player.position + direction * (spawnDistance + randomOffset);
        var enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.moveSpeed += difficulty * 0.035f;

        var health = enemy.GetComponent<Health>();
        if (health != null && difficulty >= 4)
            health.SetMaxHealth(health.maxHealth + difficulty / 4, true);

        TryMakeElite(enemy, health, difficulty);
    }

    private void TryMakeElite(EnemyController enemy, Health health, int difficulty)
    {
        if (Time.timeSinceLevelLoad < eliteStartTime || difficulty < 3)
            return;

        var eliteChance = Mathf.Clamp01(0.08f + difficulty * 0.025f);
        if (Random.value > eliteChance)
            return;

        enemy.name = "Elite Zombie";
        enemy.transform.localScale *= 1.35f;
        enemy.moveSpeed += 0.18f + difficulty * 0.025f;
        enemy.touchDamage += difficulty >= 8 ? 2 : 1;

        if (health != null)
            health.SetMaxHealth(health.maxHealth + 4 + difficulty / 2, true);

        var renderer = enemy.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.color = new Color(1f, 0.58f, 0.5f, 1f);
    }
}
