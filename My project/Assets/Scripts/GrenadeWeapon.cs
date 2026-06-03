using UnityEngine;

public class GrenadeWeapon : MonoBehaviour
{
    public bool unlocked;
    public bool evolved;
    public int level;
    public int maxLevel = 10;
    public int damage = 4;
    public float cooldown = 4.8f;
    public float range = 9f;
    public float radius = 2.2f;

    private float nextAttackTime;

    private void Update()
    {
        if (!unlocked || Time.time < nextAttackTime)
            return;

        var target = FindNearestEnemy();
        if (target == null)
            return;

        nextAttackTime = Time.time + cooldown;
        ThrowGrenade(target.position);
    }

    public void Upgrade()
    {
        unlocked = true;
        level = Mathf.Min(maxLevel, level + 1);
        damage = 3 + level;
        radius = Mathf.Min(3.6f, radius + 0.22f);
        cooldown = Mathf.Max(2.5f, cooldown * 0.88f);
    }

    public bool IsMaxLevel()
    {
        return level >= maxLevel;
    }

    public void Evolve()
    {
        if (evolved)
            return;

        evolved = true;
        damage += 4;
        radius = Mathf.Max(radius, 3.4f);
        cooldown = Mathf.Min(cooldown, 2.8f);
    }

    private Transform FindNearestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform bestTarget = null;
        var bestDistance = range;

        foreach (var enemy in enemies)
        {
            var distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance > bestDistance)
                continue;

            bestDistance = distance;
            bestTarget = enemy.transform;
        }

        return bestTarget;
    }

    private void ThrowGrenade(Vector3 targetPosition)
    {
        var direction = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        var start = transform.position + (Vector3)(direction * 0.45f);
        GrenadeProjectile.Spawn(this, start, targetPosition);
    }

    public void Detonate(Vector3 position)
    {
        var hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy"))
                continue;

            var health = hit.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);
        }

        GrenadeExplosion.Spawn(position, radius);
        BulletImpact.Spawn(position, Vector2.up);
        FloatingText.Spawn(position + Vector3.up * 0.7f, evolved ? "화염 수류탄" : "수류탄", new Color(1f, 0.65f, 0.22f, 1f));
        CameraShake.Shake(0.11f, 0.16f);

        if (evolved)
            CreateBurnZone(position);
    }

    private void CreateBurnZone(Vector3 position)
    {
        var zone = new GameObject("Grenade Burn Zone");
        zone.transform.position = position;
        zone.transform.localScale = Vector3.one * (radius * 0.82f);

        var renderer = zone.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateBurnSprite();
        renderer.color = new Color(1f, 0.23f, 0.05f, 0.58f);
        renderer.sortingOrder = 3;

        var collider = zone.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        var damageZone = zone.AddComponent<CoffeeSpillZone>();
        damageZone.damage = Mathf.Max(2, damage / 2);
        damageZone.duration = 2.8f;
    }

    private static Sprite CreateBurnSprite()
    {
        var texture = new Texture2D(48, 48, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (var y = 0; y < 48; y++)
        {
            for (var x = 0; x < 48; x++)
            {
                var nx = (x - 23.5f) / 23.5f;
                var ny = (y - 23.5f) / 23.5f;
                var distance = nx * nx + ny * ny;
                var noise = Mathf.PerlinNoise(x * 0.16f, y * 0.16f) * 0.18f;
                var alpha = distance < 0.74f + noise ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 48f, 48f), new Vector2(0.5f, 0.5f), 32f);
    }
}
