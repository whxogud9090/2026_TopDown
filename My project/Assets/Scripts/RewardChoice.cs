using UnityEngine;

public enum RewardType
{
    Damage,
    FireRate,
    MoveSpeed,
    MaxHealth,
    ProjectileSpeed,
    BookOrbit,
    CoffeeSpill,
    Grenade,
    IncendiaryGrenade,
    Shotgun
}

[System.Serializable]
public class RewardChoice
{
    public RewardType type;
    public string title;
    public string description;

    public RewardChoice(RewardType type, string title, string description)
    {
        this.type = type;
        this.title = title;
        this.description = description;
    }
}
