using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float lifeTime = 0.65f;
    public float moveSpeed = 1.1f;

    private TextMesh textMesh;
    private float timer;
    private Color startColor;

    private void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        if (textMesh != null)
            startColor = textMesh.color;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        if (textMesh != null)
        {
            var color = startColor;
            color.a = Mathf.Lerp(1f, 0f, timer / lifeTime);
            textMesh.color = color;
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    public static void Spawn(Vector3 position, string message, Color color)
    {
        var go = new GameObject("Floating Text");
        go.transform.position = position;

        var mesh = go.AddComponent<TextMesh>();
        mesh.text = message;
        mesh.fontSize = 28;
        mesh.characterSize = 0.12f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = color;

        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 31;

        go.AddComponent<FloatingText>();
    }
}
