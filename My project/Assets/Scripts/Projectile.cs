using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 1.8f;
    public int damage = 1;
    public string targetTag = "Enemy";
    public Color trailColor = new Color(1f, 0.72f, 0.24f, 1f);

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        AddTrail();
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 direction, int damageOverride, float speedOverride)
    {
        damage = damageOverride;
        speed = speedOverride;
        rb.linearVelocity = direction.normalized * speed;

        if (direction.sqrMagnitude > 0.001f)
            transform.right = direction;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag))
            return;

        var health = other.GetComponent<Health>();
        if (health != null)
            health.TakeDamage(damage);

        BulletImpact.Spawn(transform.position, rb.linearVelocity.normalized);
        Destroy(gameObject);
    }

    private void AddTrail()
    {
        var trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.08f;
        trail.startWidth = 0.18f;
        trail.endWidth = 0f;
        trail.sortingOrder = 8;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = new Color(1f, 0.86f, 0.34f, 0.95f);
        trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
    }
}
