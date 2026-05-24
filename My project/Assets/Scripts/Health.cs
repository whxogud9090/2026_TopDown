using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 3;
    public bool destroyOnDeath = true;

    public int CurrentHealth { get; private set; }
    public event Action<Health> Died;
    public event Action<Health> Changed;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        Changed?.Invoke(this);

        if (CurrentHealth <= 0)
        {
            Died?.Invoke(this);
            if (destroyOnDeath)
                Destroy(gameObject);
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0)
            return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        Changed?.Invoke(this);
    }

    public void SetMaxHealth(int value, bool refill)
    {
        maxHealth = Mathf.Max(1, value);
        CurrentHealth = refill ? maxHealth : Mathf.Min(CurrentHealth, maxHealth);
        Changed?.Invoke(this);
    }
}
