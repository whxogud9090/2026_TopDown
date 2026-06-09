using UnityEngine;

public class ExperienceDropper : MonoBehaviour
{
    public ExperienceGem gemPrefab;
    public ExperienceGem eliteGemPrefab;
    public string eliteNameText = "Elite";

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
            health.Died += OnDied;
    }

    private void OnDied(Health deadHealth)
    {
        if (SurvivorsGameManager.Instance != null)
            SurvivorsGameManager.Instance.AddKill();

        var prefab = IsEliteEnemy() && eliteGemPrefab != null ? eliteGemPrefab : gemPrefab;
        if (prefab != null)
            Instantiate(prefab, transform.position, Quaternion.identity);
    }

    private bool IsEliteEnemy()
    {
        return !string.IsNullOrEmpty(eliteNameText) && gameObject.name.Contains(eliteNameText);
    }
}
