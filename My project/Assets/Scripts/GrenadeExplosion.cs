using UnityEngine;

public class GrenadeExplosion : MonoBehaviour
{
    public float lifeTime = 0.28f;
    public float startScale = 0.35f;
    public float endScale = 2.8f;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private Color startColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            startColor = spriteRenderer.color;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        var t = Mathf.Clamp01(timer / lifeTime);
        transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

        if (spriteRenderer != null)
        {
            var color = startColor;
            color.a = Mathf.Lerp(0.9f, 0f, t);
            spriteRenderer.color = color;
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    public static void Spawn(Vector3 position, float radius)
    {
        var go = new GameObject("Grenade Explosion");
        go.transform.position = position;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite();
        renderer.color = new Color(1f, 0.42f, 0.08f, 0.9f);
        renderer.sortingOrder = 25;

        var explosion = go.AddComponent<GrenadeExplosion>();
        explosion.endScale = radius * 1.15f;
    }

    private static Sprite CreateCircleSprite()
    {
        var texture = new Texture2D(24, 24, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        var center = new Vector2(11.5f, 11.5f);

        for (var y = 0; y < 24; y++)
        {
            for (var x = 0; x < 24; x++)
            {
                var distance = Vector2.Distance(new Vector2(x, y), center);
                var color = distance <= 10.5f ? Color.white : new Color(0f, 0f, 0f, 0f);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 24f, 24f), new Vector2(0.5f, 0.5f), 16f);
    }
}
