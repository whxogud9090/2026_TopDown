using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    public float lifeTime = 0.55f;
    public float moveSpeed = 1.2f;

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

    public static void Spawn(Vector3 position, int amount)
    {
        var go = new GameObject("Damage Number");
        go.transform.position = position;

        var mesh = go.AddComponent<TextMesh>();
        mesh.text = amount.ToString();
        mesh.fontSize = 28;
        mesh.characterSize = 0.12f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = new Color(1f, 0.2f, 0.15f, 1f);

        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 30;

        go.AddComponent<DamageNumber>();
    }
}
