using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 1.8f;
    public int damage = 1;
    public string targetTag = "Enemy";

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
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

        Destroy(gameObject);
    }
}
