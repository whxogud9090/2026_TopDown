using UnityEngine;

public class WorldHealthBar : MonoBehaviour
{
    public Health targetHealth;
    public Vector3 offset = new Vector3(0f, -0.62f, 0f);
    public float width = 0.9f;
    public float height = 0.09f;

    private Transform fill;

    private void Start()
    {
        if (targetHealth == null)
            targetHealth = GetComponent<Health>();

        CreateBar();
        Refresh();

        if (targetHealth != null)
            targetHealth.Changed += _ => Refresh();
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    private void CreateBar()
    {
        var background = new GameObject("HP Bar Background");
        background.transform.SetParent(transform, false);
        background.transform.localPosition = offset;
        background.transform.localScale = new Vector3(width, height, 1f);

        var bg = background.AddComponent<SpriteRenderer>();
        bg.sprite = CreateSprite(Color.white);
        bg.color = new Color(0.08f, 0.05f, 0.05f, 0.95f);
        bg.sortingOrder = 20;

        var fillObject = new GameObject("HP Bar Fill");
        fillObject.transform.SetParent(background.transform, false);
        fillObject.transform.localPosition = new Vector3(-0.5f, 0f, -0.01f);
        fillObject.transform.localScale = new Vector3(1f, 0.72f, 1f);

        var fillRenderer = fillObject.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = CreateSprite(Color.white);
        fillRenderer.color = new Color(0.95f, 0.08f, 0.05f, 1f);
        fillRenderer.sortingOrder = 21;

        fill = fillObject.transform;
    }

    private void Refresh()
    {
        if (fill == null || targetHealth == null)
            return;

        var ratio = Mathf.Clamp01((float)targetHealth.CurrentHealth / targetHealth.maxHealth);
        fill.localScale = new Vector3(ratio, fill.localScale.y, 1f);
        fill.localPosition = new Vector3(-0.5f + ratio * 0.5f, 0f, -0.01f);
    }

    private static Sprite CreateSprite(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}
