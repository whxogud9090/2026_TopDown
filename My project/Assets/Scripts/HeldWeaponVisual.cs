using UnityEngine;

public class HeldWeaponVisual : MonoBehaviour
{
    public AutoAimWeapon weapon;
    public SpriteRenderer weaponRenderer;
    public Sprite pistolSprite;
    public Sprite shotgunSprite;
    public Transform muzzlePoint;
    public float holdDistance = 0.38f;
    public float recoilRecoverSpeed = 14f;

    private Vector2 recoilOffset;
    private float recoilAngle;

    private void LateUpdate()
    {
        if (weapon == null || weaponRenderer == null)
            return;

        var direction = weapon.CurrentAimDirection;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        weaponRenderer.sprite = weapon.shotgunUnlocked && shotgunSprite != null ? shotgunSprite : pistolSprite;

        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        recoilOffset = Vector2.Lerp(recoilOffset, Vector2.zero, recoilRecoverSpeed * Time.deltaTime);
        recoilAngle = Mathf.Lerp(recoilAngle, 0f, recoilRecoverSpeed * Time.deltaTime);

        weaponRenderer.transform.localPosition = direction.normalized * holdDistance + recoilOffset;
        weaponRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle + recoilAngle);
        weaponRenderer.flipY = direction.x < 0f;
        weaponRenderer.sortingOrder = direction.y > 0.15f ? 5 : 9;

        if (muzzlePoint != null)
            muzzlePoint.position = weaponRenderer.transform.position + (Vector3)(direction.normalized * 0.48f);
    }

    public void Kick(Vector2 direction, float amount)
    {
        if (direction.sqrMagnitude < 0.01f)
            return;

        recoilOffset = -direction.normalized * amount;
        recoilAngle = Random.Range(-7f, 7f) * Mathf.Clamp(amount * 6f, 0.6f, 1.7f);
    }
}
