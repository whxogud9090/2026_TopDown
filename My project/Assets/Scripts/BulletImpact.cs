using UnityEngine;

public class BulletImpact : MonoBehaviour
{
    public float lifeTime = 0.16f;

    private SpriteRenderer spriteRenderer;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        transform.localScale += Vector3.one * (3.2f * Time.deltaTime);

        if (spriteRenderer != null)
        {
            var color = spriteRenderer.color;
            color.a = Mathf.Lerp(1f, 0f, timer / lifeTime);
            spriteRenderer.color = color;
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    public static void Spawn(Vector3 position, Vector2 direction)
    {
        var go = new GameObject("Bullet Impact");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.34f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSprite();
        sr.color = new Color(1f, 0.14f, 0.08f, 0.92f);
        sr.sortingOrder = 12;

        go.AddComponent<BulletImpact>();

        for (var i = 0; i < 5; i++)
            BulletSpark.Spawn(position, direction);
    }

    private static Sprite CreateSprite()
    {
        var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (var y = 0; y < 16; y++)
        {
            for (var x = 0; x < 16; x++)
            {
                var dx = x - 7.5f;
                var dy = y - 7.5f;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                var alpha = distance < 6.3f ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
    }
}
