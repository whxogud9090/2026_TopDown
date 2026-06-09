using UnityEngine;

public class SupplySpawner : MonoBehaviour
{
    public SupplyPickup healPrefab;
    public SupplyPickup bombPrefab;
    public Transform player;
    public float spawnInterval = 18f;
    public float spawnDistance = 7.5f;
    public float bombStartTime = 90f;
    public int bombMinEnemyCount = 18;
    public int maxPickups = 5;
    public int maxHealPickups = 2;

    private float nextSpawnTime;

    private void Start()
    {
        nextSpawnTime = Time.time + 8f;
    }

    private void Update()
    {
        if (player == null || Time.time < nextSpawnTime)
            return;

        if (FindObjectsByType<SupplyPickup>(FindObjectsSortMode.None).Length >= maxPickups)
        {
            nextSpawnTime = Time.time + 3f;
            return;
        }

        nextSpawnTime = Time.time + spawnInterval;
        SpawnRandomPickup();
    }

    private void SpawnRandomPickup()
    {
        var prefab = ChoosePrefab();
        if (prefab == null)
            return;

        var direction = Random.insideUnitCircle.normalized;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        var position = (Vector2)player.position + direction * Random.Range(spawnDistance * 0.55f, spawnDistance);
        Instantiate(prefab, position, Quaternion.identity);
    }

    private SupplyPickup ChoosePrefab()
    {
        var bombAllowed = Time.timeSinceLevelLoad >= bombStartTime
            && GameObject.FindGameObjectsWithTag("Enemy").Length >= bombMinEnemyCount;
        var healAllowed = CountPickups(SupplyPickupType.Heal) < maxHealPickups;

        var roll = Random.value;
        if (!bombAllowed)
        {
            if (healAllowed && roll < 0.28f)
                return healPrefab;

            return healAllowed ? healPrefab : null;
        }

        if (healAllowed && roll < 0.24f)
            return healPrefab;

        return bombPrefab;
    }

    private int CountPickups(SupplyPickupType pickupType)
    {
        var count = 0;
        var pickups = FindObjectsByType<SupplyPickup>(FindObjectsSortMode.None);
        foreach (var pickup in pickups)
        {
            if (pickup.type == pickupType)
                count++;
        }

        return count;
    }
}
