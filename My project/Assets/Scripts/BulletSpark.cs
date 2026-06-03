using UnityEngine;

public class BulletSpark : MonoBehaviour
{
    public float lifeTime = 0.18f;
    public Vector2 velocity;

    private SpriteRenderer spriteRenderer;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        velocity = Vector2.Lerp(velocity, Vector2.zero, 7f * Time.deltaTime);

        if (spriteRenderer != null)
        {
            var color = spriteRenderer.color;
            color.a = Mathf.Lerp(1f, 0f, timer / lifeTime);
            spriteRenderer.color = color;
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    public static void Spawn(Vector3 position, Vector2 hitDirection)
    {
        if (hitDirection.sqrMagnitude < 0.01f)
            hitDirection = Vector2.right;

        var angle = Random.Range(120f, 240f);
        var sparkDirection = Quaternion.Euler(0f, 0f, angle) * hitDirection.normalized;

        var go = new GameObject("Bullet Spark");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * Random.Range(0.05f, 0.09f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSprite();
        sr.color = Random.value > 0.45f
            ? new Color(1f, 0.72f, 0.22f, 1f)
            : new Color(0.95f, 0.08f, 0.05f, 1f);
        sr.sortingOrder = 14;

        var spark = go.AddComponent<BulletSpark>();
        spark.velocity = sparkDirection * Random.Range(1.6f, 3.2f);
    }

    private static Sprite CreateSprite()
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (var y = 0; y < 2; y++)
        {
            for (var x = 0; x < 2; x++)
                texture.SetPixel(x, y, Color.white);
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);
    }
}
