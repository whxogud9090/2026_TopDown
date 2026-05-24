using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    public Projectile projectilePrefab;
    public Transform firePoint;
    public float fireCooldown = 0.28f;
    public float projectileSpeed = 9f;
    public int damage = 1;

    private Camera mainCamera;
    private float nextFireTime;
    private bool attackHeld;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (firePoint == null)
            firePoint = transform;
    }

    public void OnAttack(InputValue value)
    {
        attackHeld = value.isPressed;
        if (attackHeld)
            TryFire();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            attackHeld = true;

        if (attackHeld)
            TryFire();

        if (Mouse.current != null && !Mouse.current.leftButton.isPressed)
            attackHeld = false;
    }

    private void TryFire()
    {
        if (Time.time < nextFireTime || projectilePrefab == null)
            return;

        nextFireTime = Time.time + fireCooldown;
        var direction = GetAimDirection();
        var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        projectile.Launch(direction, damage, projectileSpeed);
    }

    private Vector2 GetAimDirection()
    {
        if (mainCamera != null && Mouse.current != null)
        {
            var mousePosition = Mouse.current.position.ReadValue();
            var world = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));
            var direction = (Vector2)(world - transform.position);
            if (direction.sqrMagnitude > 0.001f)
                return direction.normalized;
        }

        return Vector2.right;
    }

    public void ApplyUpgrade(RewardType rewardType)
    {
        switch (rewardType)
        {
            case RewardType.Damage:
                damage += 1;
                break;
            case RewardType.FireRate:
                fireCooldown = Mathf.Max(0.08f, fireCooldown * 0.82f);
                break;
            case RewardType.ProjectileSpeed:
                projectileSpeed += 1.5f;
                break;
        }
    }
}
