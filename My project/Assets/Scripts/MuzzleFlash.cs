using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public float lifeTime = 0.08f;

    private SpriteRenderer spriteRenderer;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        transform.localScale += Vector3.one * (5f * Time.deltaTime);

        if (spriteRenderer != null)
        {
            var color = spriteRenderer.color;
            color.a = Mathf.Lerp(1f, 0f, timer / lifeTime);
            spriteRenderer.color = color;
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    public static void Spawn(Vector3 position, Vector2 direction, int power = 1)
    {
        var go = new GameObject("Muzzle Flash");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * (power == 1 ? 0.30f : 0.55f);

        if (direction.sqrMagnitude > 0.01f)
            go.transform.right = direction;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSprite();
        sr.color = power == 1 ? new Color(1f, 0.72f, 0.18f, 0.95f) : new Color(1f, 0.45f, 0.12f, 0.95f);
        sr.sortingOrder = 16;

        go.AddComponent<MuzzleFlash>();
    }

    private static Sprite CreateSprite()
    {
        var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (var y = 0; y < 16; y++)
        {
            for (var x = 0; x < 16; x++)
            {
                var dx = x - 2f;
                var dy = y - 7.5f;
                var distance = Mathf.Sqrt(dx * dx * 0.6f + dy * dy);
                var alpha = distance < 5.2f && x > 1 ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.1f, 0.5f), 16f);
    }
}
