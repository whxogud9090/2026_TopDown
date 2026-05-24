using System.Collections.Generic;
using UnityEngine;

public class OrbitDamageZone : MonoBehaviour
{
    public int damage = 1;
    public float hitCooldown = 0.35f;
    public string targetTag = "Enemy";

    private readonly Dictionary<Health, float> lastHitTimes = new();

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag))
            return;

        var health = other.GetComponent<Health>();
        if (health == null)
            return;

        if (lastHitTimes.TryGetValue(health, out var lastHitTime) && Time.time < lastHitTime + hitCooldown)
            return;

        lastHitTimes[health] = Time.time;
        health.TakeDamage(damage);
    }
}
