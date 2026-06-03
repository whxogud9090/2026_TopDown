using UnityEngine;

public class ShellCasing : MonoBehaviour
{
    public float lifeTime = 0.75f;
    public Vector2 velocity;
    public float spinSpeed;

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
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        velocity = Vector2.Lerp(velocity, Vector2.zero, 3f * Time.deltaTime);

        if (spriteRenderer != null)
        {
            var color = spriteRenderer.color;
            color.a = Mathf.Lerp(1f, 0f, timer / lifeTime);
            spriteRenderer.color = color;
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    public static void Spawn(Vector3 position, Vector2 shotDirection)
    {
        if (shotDirection.sqrMagnitude < 0.01f)
            shotDirection = Vector2.right;

        var right = new Vector2(shotDirection.y, -shotDirection.x).normalized;
        if (Random.value > 0.5f)
            right = -right;

        var go = new GameObject("Shell Casing");
        go.transform.position = position + (Vector3)(right * 0.22f);
        go.transform.localScale = new Vector3(0.16f, 0.06f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSprite();
        sr.color = new Color(0.95f, 0.68f, 0.28f, 1f);
        sr.sortingOrder = 13;

        var casing = go.AddComponent<ShellCasing>();
        casing.velocity = right * Random.Range(1.2f, 2.2f) - shotDirection.normalized * 0.45f;
        casing.spinSpeed = Random.Range(280f, 520f);
    }

    private static Sprite CreateSprite()
    {
        var texture = new Texture2D(8, 4, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
                texture.SetPixel(x, y, Color.white);
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 8f, 4f), new Vector2(0.5f, 0.5f), 8f);
    }
}
