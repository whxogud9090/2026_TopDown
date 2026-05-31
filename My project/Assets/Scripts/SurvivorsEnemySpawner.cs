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

    private float nextSpawnTime;

    private void Update()
    {
        if (enemyPrefab == null || player == null || Time.time < nextSpawnTime)
            return;

        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies)
            return;

        var difficulty = GetDifficultyLevel();
        var currentInterval = Mathf.Max(minSpawnInterval, spawnInterval - difficulty * 0.08f);
        nextSpawnTime = Time.time + currentInterval;
        maxEnemies = Mathf.Min(maxEnemiesLimit, 80 + difficulty * 12);

        var spawnCount = Mathf.Min(6, 1 + difficulty / 2);
        for (var i = 0; i < spawnCount; i++)
            SpawnEnemy(difficulty);
    }

    private int GetDifficultyLevel()
    {
        return Mathf.FloorToInt(Time.timeSinceLevelLoad / difficultyStepTime);
    }

    private void SpawnEnemy(int difficulty)
    {
        var direction = Random.insideUnitCircle.normalized;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        var randomOffset = Random.Range(-1.6f, 1.6f);
        var position = (Vector2)player.position + direction * (spawnDistance + randomOffset);
        var enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.moveSpeed += difficulty * 0.05f;

        var health = enemy.GetComponent<Health>();
        if (health != null && difficulty >= 4)
            health.SetMaxHealth(health.maxHealth + difficulty / 4, true);
    }
}
