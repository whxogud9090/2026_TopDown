using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;

    private float timeLeft;
    private float strength;
    private Vector3 lastShakeOffset;

    private void Awake()
    {
        instance = this;
    }

    private void LateUpdate()
    {
        transform.position -= lastShakeOffset;
        lastShakeOffset = Vector3.zero;

        if (timeLeft <= 0f)
            return;

        timeLeft -= Time.deltaTime;
        var shake = Random.insideUnitCircle * strength;
        lastShakeOffset = new Vector3(shake.x, shake.y, 0f);
        transform.position += lastShakeOffset;

        if (timeLeft <= 0f)
            strength = 0f;
    }

    public static void Shake(float duration, float amount)
    {
        if (instance == null)
            return;

        instance.timeLeft = Mathf.Max(instance.timeLeft, duration);
        instance.strength = Mathf.Max(instance.strength, amount);
    }
}
