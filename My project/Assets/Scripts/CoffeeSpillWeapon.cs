using UnityEngine;

public class CoffeeSpillWeapon : MonoBehaviour
{
    public Sprite spillSprite;
    public bool unlocked;
    public int level;
    public int damage = 1;
    public float cooldown = 5.2f;
    public float radius = 1.85f;
    public float duration = 3f;
    public float targetRange = 12f;

    private float nextSpillTime;
    private static Sprite generatedSpillSprite;

    private void Update()
    {
        if (!unlocked || Time.time < nextSpillTime)
            return;

        var target = FindNearestEnemy();
        if (target == null)
            return;

        nextSpillTime = Time.time + cooldown;
        CreateSpill(target.position);
    }

    public void Upgrade()
    {
        unlocked = true;
        level++;
        damage = Mathf.Max(1, damage + (level > 1 ? 1 : 0));
        radius = Mathf.Min(3.2f, radius + 0.25f);
        cooldown = Mathf.Max(2.4f, cooldown * 0.86f);
    }

    private Transform FindNearestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform best = null;
        var bestDistance = targetRange * targetRange;

        foreach (var enemy in enemies)
        {
            var sqrDistance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance >= bestDistance)
                continue;

            bestDistance = sqrDistance;
            best = enemy.transform;
        }

        return best;
    }

    private void CreateSpill(Vector3 position)
    {
        var spill = new GameObject("Hot Coffee Spill");
        spill.transform.position = position;
        spill.transform.localScale = Vector3.one * radius;

        var sr = spill.AddComponent<SpriteRenderer>();
        sr.sprite = spillSprite != null ? spillSprite : GetGeneratedSpillSprite();
        sr.color = new Color(1f, 0.34f, 0.08f, 0.62f);
        sr.sortingOrder = 2;

        var collider = spill.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        var zone = spill.AddComponent<CoffeeSpillZone>();
        zone.damage = damage;
        zone.duration = duration;
    }

    private static Sprite GetGeneratedSpillSprite()
    {
        if (generatedSpillSprite != null)
            return generatedSpillSprite;

        var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var nx = (x - 31.5f) / 31.5f;
                var ny = (y - 31.5f) / 31.5f;
                var distance = nx * nx * 0.85f + ny * ny * 1.18f;
                var edgeNoise = Mathf.PerlinNoise(x * 0.18f, y * 0.18f) * 0.22f;
                var alpha = distance < 0.72f + edgeNoise ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        generatedSpillSprite = Sprite.Create(texture, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 32f);
        return generatedSpillSprite;
    }
}
