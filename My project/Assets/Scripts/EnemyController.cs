using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 1.6f;
    public int touchDamage = 1;
    public float attackCooldown = 0.7f;
    public bool charger;
    public float chargeDistance = 3f;
    public float chargeMultiplier = 2.5f;

    private Rigidbody2D rb;
    private Transform target;
    private float nextAttackTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        var direction = ((Vector2)target.position - rb.position).normalized;
        var distance = Vector2.Distance(target.position, rb.position);
        var speed = charger && distance < chargeDistance ? moveSpeed * chargeMultiplier : moveSpeed;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player") || Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;
        var health = collision.collider.GetComponent<Health>();
        if (health != null)
            health.TakeDamage(touchDamage);
    }
}
