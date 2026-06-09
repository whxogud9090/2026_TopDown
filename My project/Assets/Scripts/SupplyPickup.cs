using UnityEngine;

public enum SupplyPickupType
{
    Heal,
    Bomb,
    Experience
}

public class SupplyPickup : MonoBehaviour
{
    public SupplyPickupType type;
    public int healAmount = 2;
    public int experienceAmount = 4;
    public int bombDamage = 999;
    public float pickupDistance = 0.65f;
    public float magnetDistance = 2.4f;
    public float magnetSpeed = 4.8f;
    public float lifeTime = 22f;
    public float rotateSpeed = 95f;
    public float bobHeight = 0.12f;
    public float bobSpeed = 4f;

    private Transform player;
    private float timer;
    private Vector3 startPosition;
    private bool pickedUp;

    private void Awake()
    {
        var colliders = GetComponents<Collider2D>();
        foreach (var pickupCollider in colliders)
            pickupCollider.isTrigger = true;

        var body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.bodyType = RigidbodyType2D.Kinematic;
            body.simulated = true;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }

    private void Start()
    {
        startPosition = transform.position;

        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
        transform.position = startPosition + Vector3.up * (Mathf.Sin(timer * bobSpeed) * bobHeight);

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        if (player == null)
            return;

        var distance = Vector2.Distance(transform.position, player.position);
        if (distance <= pickupDistance)
        {
            PickUp();
            return;
        }

        if (magnetDistance > 0f && distance <= magnetDistance)
        {
            var nextPosition = Vector2.MoveTowards(transform.position, player.position, magnetSpeed * Time.deltaTime);
            transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
            startPosition = transform.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryPickupFromCollider(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryPickupFromCollider(other);
    }

    private void TryPickupFromCollider(Collider2D other)
    {
        if (pickedUp || !other.CompareTag("Player"))
            return;

        if (player == null)
            player = other.transform;

        PickUp();
    }

    private void PickUp()
    {
        if (pickedUp)
            return;

        pickedUp = true;

        if (type == SupplyPickupType.Heal)
        {
            var health = player.GetComponent<Health>();
            if (health != null && health.CurrentHealth < health.maxHealth)
            {
                health.Heal(healAmount);
                FloatingText.Spawn(transform.position + Vector3.up * 0.45f, "+HP", new Color(0.35f, 1f, 0.48f, 1f));
            }
            else
            {
                FloatingText.Spawn(transform.position + Vector3.up * 0.45f, "HP FULL", new Color(0.72f, 1f, 0.72f, 1f));
            }
        }
        else if (type == SupplyPickupType.Bomb)
        {
            KillVisibleEnemies();
            FloatingText.Spawn(transform.position + Vector3.up * 0.45f, "BOOM!", new Color(1f, 0.45f, 0.12f, 1f));
        }
        else if (type == SupplyPickupType.Experience)
        {
            SurvivorsGameManager.Instance.AddExperience(experienceAmount);
            FloatingText.Spawn(transform.position + Vector3.up * 0.45f, "+XP", new Color(0.45f, 0.9f, 1f, 1f));
        }

        Destroy(gameObject);
    }

    private void KillVisibleEnemies()
    {
        var camera = Camera.main;
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in enemies)
        {
            if (camera != null)
            {
                var point = camera.WorldToViewportPoint(enemy.transform.position);
                var onScreen = point.x >= -0.08f && point.x <= 1.08f
                    && point.y >= -0.08f && point.y <= 1.08f
                    && point.z > 0f;

                if (!onScreen)
                    continue;
            }

            var health = enemy.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(bombDamage);
        }

        BulletImpact.Spawn(transform.position, Vector2.up);
        CameraShake.Shake(0.18f, 0.22f);
    }
}
