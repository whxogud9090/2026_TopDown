using UnityEngine;

public class BossDropper : MonoBehaviour
{
    public SupplyPickup healPrefab;
    public SupplyPickup bombPrefab;
    public SupplyPickup experiencePrefab;
    public int experienceDropCount = 2;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
            health.Died += OnDied;
    }

    private void OnDied(Health deadHealth)
    {
        Drop(experiencePrefab, new Vector2(-0.7f, 0.1f));
        Drop(experiencePrefab, new Vector2(0.7f, 0.1f));
        Drop(healPrefab, new Vector2(0f, 0.75f));
        Drop(bombPrefab, new Vector2(0f, -0.75f));

        for (var i = 2; i < experienceDropCount; i++)
        {
            var angle = i * 137.5f * Mathf.Deg2Rad;
            Drop(experiencePrefab, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.9f);
        }

        FloatingText.Spawn(transform.position + Vector3.up * 1.1f, "보급품 확보", new Color(1f, 0.78f, 0.28f, 1f));
        CameraShake.Shake(0.16f, 0.18f);
    }

    private void Drop(SupplyPickup prefab, Vector2 offset)
    {
        if (prefab == null)
            return;

        Instantiate(prefab, (Vector2)transform.position + offset, Quaternion.identity);
    }
}
