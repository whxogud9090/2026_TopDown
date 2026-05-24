using UnityEngine;

public class SurvivorsEnemySpawner : MonoBehaviour
{
    public EnemyController enemyPrefab;
    public Transform player;
    public float spawnInterval = 1.2f;
    public float spawnDistance = 11f;
    public int maxEnemies = 80;

    private float nextSpawnTime;

    private void Update()
    {
        if (enemyPrefab == null || player == null || Time.time < nextSpawnTime)
            return;

        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies)
            return;

        nextSpawnTime = Time.time + spawnInterval;
        var direction = Random.insideUnitCircle.normalized;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        var position = (Vector2)player.position + direction * spawnDistance;
        Instantiate(enemyPrefab, position, Quaternion.identity);
    }
}
