using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    public float lifeTime = 0.045f;

    private LineRenderer line;
    private float timer;
    private Color startColor;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        startColor = new Color(1f, 0.86f, 0.32f, 0.9f);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        var alpha = Mathf.Lerp(0.9f, 0f, timer / lifeTime);
        var color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (line != null)
        {
            line.startColor = color;
            line.endColor = new Color(1f, 0.35f, 0.08f, 0f);
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    public static void Spawn(Vector3 start, Vector2 direction, int power)
    {
        if (direction.sqrMagnitude < 0.01f)
            return;

        var go = new GameObject("Bullet Tracer");
        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = 15;
        line.startWidth = power == 1 ? 0.06f : 0.09f;
        line.endWidth = 0f;
        line.startColor = new Color(1f, 0.86f, 0.32f, 0.9f);
        line.endColor = new Color(1f, 0.35f, 0.08f, 0f);

        var length = power == 1 ? 1.65f : 2.25f;
        line.SetPosition(0, start);
        line.SetPosition(1, start + (Vector3)(direction.normalized * length));

        go.AddComponent<BulletTracer>();
    }
}
