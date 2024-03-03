public struct PlayerStats
{
    public float EnginePower;
    public float RotationSpeed;
    public int MaxHealth;
    public int Health;

    public int ShieldMaxHealth;
    public int ShieldHealth;

    public static PlayerStats Default() => new()
    {
        EnginePower = 2000f,
        RotationSpeed = 25f,
        Health = 10,
        MaxHealth = 10,
        ShieldMaxHealth = 3,
        ShieldHealth = 3
    };
}
