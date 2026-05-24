using UnityEngine;

public class TouchKillPlayer : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryKill(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryKill(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other);
    }

    private void TryKill(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        var health = other.GetComponent<Health>();
        if (health != null)
            health.TakeDamage(999);
    }
}
