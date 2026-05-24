using System.Collections.Generic;
using UnityEngine;

public class CoffeeSpillZone : MonoBehaviour
{
    public int damage = 1;
    public float duration = 3f;
    public float tickInterval = 0.45f;
    public string targetTag = "Enemy";

    private readonly Dictionary<Health, float> lastTickTimes = new();
    private float remaining;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        remaining = duration;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        remaining -= Time.deltaTime;
        if (spriteRenderer != null)
        {
            var alpha = Mathf.Clamp01(remaining / duration) * 0.55f;
            spriteRenderer.color = new Color(1f, 0.34f, 0.08f, alpha);
        }

        if (remaining <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag))
            return;

        var health = other.GetComponent<Health>();
        if (health == null)
            return;

        if (lastTickTimes.TryGetValue(health, out var lastTickTime) && Time.time < lastTickTime + tickInterval)
            return;

        lastTickTimes[health] = Time.time;
        health.TakeDamage(damage);
    }
}
