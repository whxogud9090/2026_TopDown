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

    private float nextFireTime;
    private float nextShotgunTime;

    private void Update()
    {
        if (Time.time < nextFireTime || projectilePrefab == null)
            return;

        var target = FindNearestEnemy();
        if (target == null)
            return;

        var direction = (target.position - transform.position).normalized;

        nextFireTime = Time.time + fireCooldown;
        FireBullet(direction, damage, projectileSpeed);

        if (shotgunUnlocked && Time.time >= nextShotgunTime)
        {
            nextShotgunTime = Time.time + shotgunCooldown;
            FireShotgun(direction);
        }
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
            FireBullet(direction, damage, projectileSpeed * 0.95f);
            return;
        }

        for (var i = 0; i < shotgunPellets; i++)
        {
            var t = shotgunPellets == 1 ? 0.5f : (float)i / (shotgunPellets - 1);
            var angle = Mathf.Lerp(-shotgunSpreadAngle, shotgunSpreadAngle, t);
            var spreadDirection = Quaternion.Euler(0f, 0f, angle) * direction;
            FireBullet(spreadDirection, Mathf.Max(1, damage), projectileSpeed * 0.9f);
        }
    }

    private void FireBullet(Vector2 direction, int bulletDamage, float bulletSpeed)
    {
        var projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectile.Launch(direction, bulletDamage, bulletSpeed);
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
