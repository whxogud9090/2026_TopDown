using UnityEngine;

public class AutoAimWeapon : MonoBehaviour
{
    public Projectile projectilePrefab;
    public float fireCooldown = 0.55f;
    public float projectileSpeed = 8.5f;
    public float range = 9f;
    public int damage = 1;

    private float nextFireTime;

    private void Update()
    {
        if (Time.time < nextFireTime || projectilePrefab == null)
            return;

        var target = FindNearestEnemy();
        if (target == null)
            return;

        nextFireTime = Time.time + fireCooldown;
        var direction = (target.position - transform.position).normalized;
        var projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectile.Launch(direction, damage, projectileSpeed);
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
