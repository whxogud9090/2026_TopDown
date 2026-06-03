using UnityEngine;

public class GrenadeProjectile : MonoBehaviour
{
    public GrenadeWeapon owner;
    public Vector3 startPosition;
    public Vector3 targetPosition;
    public float flyTime = 0.48f;
    public float arcHeight = 1.35f;
    public float spinSpeed = 720f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        var t = Mathf.Clamp01(timer / flyTime);
        var flatPosition = Vector3.Lerp(startPosition, targetPosition, t);
        var arc = Mathf.Sin(t * Mathf.PI) * arcHeight;

        transform.position = flatPosition + Vector3.up * arc;
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        if (t >= 1f)
        {
            if (owner != null)
                owner.Detonate(targetPosition);

            Destroy(gameObject);
        }
    }

    public static void Spawn(GrenadeWeapon owner, Vector3 start, Vector3 target)
    {
        var go = new GameObject("Thrown Grenade");
        go.transform.position = start;
        go.transform.localScale = Vector3.one * 0.85f;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateGrenadeSprite();
        renderer.color = new Color(0.42f, 0.72f, 0.34f, 1f);
        renderer.sortingOrder = 12;

        var projectile = go.AddComponent<GrenadeProjectile>();
        projectile.owner = owner;
        projectile.startPosition = start;
        projectile.targetPosition = target;
    }

    private static Sprite CreateGrenadeSprite()
    {
        var texture = new Texture2D(18, 18, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (var y = 0; y < 18; y++)
        {
            for (var x = 0; x < 18; x++)
            {
                var color = new Color(0f, 0f, 0f, 0f);
                var body = x >= 5 && x <= 13 && y >= 4 && y <= 14
                    && Mathf.Abs(x - 9) + Mathf.Abs(y - 9) <= 8;
                var outline = body && (x <= 5 || x >= 13 || y <= 4 || y >= 14);
                var pin = x >= 8 && x <= 12 && y >= 13 && y <= 16;
                var shine = body && x >= 7 && x <= 8 && y >= 9 && y <= 12;

                if (body)
                    color = new Color(0.25f, 0.45f, 0.2f, 1f);
                if (outline)
                    color = new Color(0.06f, 0.12f, 0.06f, 1f);
                if (pin)
                    color = new Color(0.18f, 0.18f, 0.15f, 1f);
                if (shine)
                    color = new Color(0.62f, 0.9f, 0.48f, 1f);

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 18f, 18f), new Vector2(0.5f, 0.5f), 16f);
    }
}
