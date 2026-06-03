using UnityEngine;

public class AutoAimWeapon : MonoBehaviour
{
    public Projectile projectilePrefab;
    public float fireCooldown = 0.55f;
    public float projectileSpeed = 8.5f;
    public float range = 9f;
    public int damage = 1;
    public bool shotgunUnlocked;
    public int shotgunPellets = 4;
    public float shotgunCooldown = 1.7f;
    public float shotgunSpreadAngle = 34f;
    public Transform muzzlePoint;
    public Vector2 CurrentAimDirection { get; private set; } = Vector2.right;
    public int ShotPower { get; private set; }

    private float nextFireTime;
    private float nextShotgunTime;
    private HeldWeaponVisual heldWeaponVisual;

    private void Awake()
    {
        heldWeaponVisual = GetComponent<HeldWeaponVisual>();
    }

    private void Update()
    {
        if (Time.time < nextFireTime || projectilePrefab == null)
            return;

        var target = FindNearestEnemy();
        if (target == null)
            return;

        var aimStart = muzzlePoint != null ? muzzlePoint.position : transform.position;
        var direction = (target.position - aimStart).normalized;
        CurrentAimDirection = direction;

        if (shotgunUnlocked)
        {
            if (Time.time < nextShotgunTime)
                return;

            nextFireTime = Time.time + shotgunCooldown;
            nextShotgunTime = Time.time + shotgunCooldown;
            FireShotgun(direction);
            return;
        }

        nextFireTime = Time.time + fireCooldown;
        FireBullet(direction, damage, projectileSpeed, 1);
    }

    public void UnlockShotgun()
    {
        shotgunUnlocked = true;
        shotgunPellets = Mathf.Min(8, shotgunPellets + 1);
        shotgunCooldown = Mathf.Max(0.75f, shotgunCooldown * 0.9f);
    }

    private void FireShotgun(Vector2 direction)
    {
        if (shotgunPellets <= 1)
        {
            FireBullet(direction, damage, projectileSpeed * 0.95f, 2);
            return;
        }

        MuzzleFlash.Spawn(muzzlePoint != null ? muzzlePoint.position : transform.position, direction, 2);
        CameraShake.Shake(0.12f, 0.16f);
        ShellCasing.Spawn(transform.position, direction);
        if (heldWeaponVisual != null)
            heldWeaponVisual.Kick(direction, 0.26f);

        for (var i = 0; i < shotgunPellets; i++)
        {
            var t = shotgunPellets == 1 ? 0.5f : (float)i / (shotgunPellets - 1);
            var angle = Mathf.Lerp(-shotgunSpreadAngle, shotgunSpreadAngle, t);
            var spreadDirection = Quaternion.Euler(0f, 0f, angle) * direction;
            FireBullet(spreadDirection, Mathf.Max(1, damage), projectileSpeed * 0.9f, 0);
        }
    }

    private void FireBullet(Vector2 direction, int bulletDamage, float bulletSpeed, int shotPower)
    {
        var firePosition = muzzlePoint != null ? muzzlePoint.position : transform.position;
        var projectile = Instantiate(projectilePrefab, firePosition, Quaternion.identity);
        projectile.Launch(direction, bulletDamage, bulletSpeed);

        if (shotPower > 0)
        {
            ShotPower = shotPower;
            MuzzleFlash.Spawn(firePosition, direction, shotPower);
            BulletTracer.Spawn(firePosition, direction, shotPower);
            CameraShake.Shake(shotPower == 1 ? 0.055f : 0.12f, shotPower == 1 ? 0.055f : 0.16f);
            ShellCasing.Spawn(transform.position, direction);

            if (heldWeaponVisual != null)
                heldWeaponVisual.Kick(direction, shotPower == 1 ? 0.12f : 0.26f);
        }
    }

    private Transform FindNearestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform best = null;
        var bestDistance = range * range;

        foreach (var enemy in enemies)
        {
            var sqrDistance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance >= bestDistance)
                continue;

            bestDistance = sqrDistance;
            best = enemy.transform;
        }

        return best;
    }
}
