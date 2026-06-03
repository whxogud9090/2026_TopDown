using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public EnemyController bossPrefab;
    public Transform player;
    public float firstSpawnTime = 65f;
    public float spawnInterval = 95f;
    public float minSpawnInterval = 55f;
    public float spawnDistance = 16f;

    private float nextSpawnTime;
    private int bossWave;

    private void Start()
    {
        nextSpawnTime = Time.time + firstSpawnTime;
    }

    private void Update()
    {
        if (player == null || Time.time < nextSpawnTime)
            return;

        if (FindFirstObjectByType<BossEnemy>() != null)
        {
            nextSpawnTime = Time.time + 5f;
            return;
        }

        SpawnBoss();
        var interval = Mathf.Max(minSpawnInterval, spawnInterval - bossWave * 7f);
        nextSpawnTime = Time.time + interval;
    }

    private void SpawnBoss()
    {
        bossWave++;

        var direction = Random.insideUnitCircle.normalized;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        var position = (Vector2)player.position + direction * spawnDistance;
        var boss = Instantiate(bossPrefab, position, Quaternion.identity);

        boss.moveSpeed += bossWave * 0.08f;
        boss.touchDamage += bossWave >= 3 ? 1 : 0;

        var health = boss.GetComponent<Health>();
        if (health != null)
            health.SetMaxHealth(health.maxHealth + bossWave * 8, true);

        FloatingText.Spawn(position + Vector2.up * 1.6f, "중간 보스 등장", new Color(1f, 0.22f, 0.12f, 1f));
        CameraShake.Shake(0.14f, 0.14f);
    }
}
