using Microsoft.Xna.Framework;
using System;

public struct PlayerStats
{
    public float EnginePower;
    public float RotationSpeed;
    public int MaxHealth;
    public int Health;
    public double HealthRegenRate; // in seconds
    public double _lastHealthRegenTime;

    public int ShieldMaxPower;
    public int ShieldPower;
    public double ShieldRechargeRate; // in seconds
    private double _lastShieldRechargeTime;

    public bool IsAlive => Health > 0;
    public bool HasShield => ShieldPower > 0;

    public void Update(GameTime gameTime)
    {
        if (HealthRegenRate > 0f && Health < MaxHealth &&
            gameTime.TotalGameTime.TotalSeconds - _lastHealthRegenTime > HealthRegenRate)
        {
            _lastHealthRegenTime = gameTime.TotalGameTime.TotalSeconds;
            Health = Math.Min(Health + 1, this.MaxHealth);
        }

        if (ShieldRechargeRate > 0f && ShieldPower < ShieldMaxPower &&
            gameTime.TotalGameTime.TotalSeconds - _lastShieldRechargeTime > ShieldRechargeRate)
        {
            _lastShieldRechargeTime = gameTime.TotalGameTime.TotalSeconds;
            ShieldPower = Math.Min(ShieldPower+1, this.ShieldMaxPower);
        }
    }

    public static PlayerStats Create(int maxHealth = 10, int maxShields = 10, float enginePower = 2000f) => new()
    {
        EnginePower = enginePower,
        RotationSpeed = 25f,
        Health = maxHealth,
        MaxHealth = maxHealth,
        ShieldMaxPower = maxShields,
        ShieldPower = maxShields,
        HealthRegenRate = 20,
        ShieldRechargeRate = 10
    };
}
