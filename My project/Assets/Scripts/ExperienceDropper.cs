using UnityEngine;

public class ExperienceDropper : MonoBehaviour
{
    public ExperienceGem gemPrefab;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
            health.Died += OnDied;
    }

    private void OnDied(Health deadHealth)
    {
        if (gemPrefab != null)
            Instantiate(gemPrefab, transform.position, Quaternion.identity);
    }
}
