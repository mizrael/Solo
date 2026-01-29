using System;

namespace Solocaster.Character;

public enum MetricType
{
    MeleeAttack,
    RangedAttack,
    DamageTaken,
    DamageBlocked,
    EnemyKilled,
    SpellCast,
    MagicDamage,
    Healing,
    Walking,
    Running,
    Sneaking,
    NPCInteraction,
    ItemBought,
    ItemSold,
    LockPick,
    PotionUsed,
    ScrollUsed
}

public class PlayerMetrics
{
    // Combat metrics
    public int MeleeAttacks { get; private set; }
    public float MeleeDamageDealt { get; private set; }
    public int RangedAttacks { get; private set; }
    public float RangedDamageDealt { get; private set; }
    public float DamageTaken { get; private set; }
    public float DamageBlocked { get; private set; }
    public int EnemiesKilled { get; private set; }

    // Magic metrics
    public int SpellsCast { get; private set; }
    public float MagicDamageDealt { get; private set; }
    public float HealingDone { get; private set; }
    public float ManaSpent { get; private set; }

    // Movement metrics
    public float DistanceWalked { get; private set; }
    public float TimeWalked { get; private set; }
    public float DistanceRun { get; private set; }
    public float TimeRun { get; private set; }
    public float TimeSneaking { get; private set; }

    // Social/Trade metrics
    public int NPCInteractions { get; private set; }
    public int ItemsBought { get; private set; }
    public int ItemsSold { get; private set; }
    public int GoldSpent { get; private set; }
    public int GoldEarned { get; private set; }
    public int LocksPickedSuccessfully { get; private set; }
    public int LockPickAttempts { get; private set; }

    // Item usage
    public int PotionsUsed { get; private set; }
    public int ScrollsUsed { get; private set; }
    public int ItemsPickedUp { get; private set; }

    public event Action<MetricType, float>? OnMetricChanged;

    public void RecordMeleeAttack(float damage)
    {
        MeleeAttacks++;
        MeleeDamageDealt += damage;
        OnMetricChanged?.Invoke(MetricType.MeleeAttack, damage);
    }

    public void RecordRangedAttack(float damage)
    {
        RangedAttacks++;
        RangedDamageDealt += damage;
        OnMetricChanged?.Invoke(MetricType.RangedAttack, damage);
    }

    public void RecordDamageTaken(float damage)
    {
        DamageTaken += damage;
        OnMetricChanged?.Invoke(MetricType.DamageTaken, damage);
    }

    public void RecordDamageBlocked(float damage)
    {
        DamageBlocked += damage;
        OnMetricChanged?.Invoke(MetricType.DamageBlocked, damage);
    }

    public void RecordEnemyKilled()
    {
        EnemiesKilled++;
        OnMetricChanged?.Invoke(MetricType.EnemyKilled, 1);
    }

    public void RecordSpellCast(float manaCost)
    {
        SpellsCast++;
        ManaSpent += manaCost;
        OnMetricChanged?.Invoke(MetricType.SpellCast, manaCost);
    }

    public void RecordMagicDamage(float damage)
    {
        MagicDamageDealt += damage;
        OnMetricChanged?.Invoke(MetricType.MagicDamage, damage);
    }

    public void RecordHealing(float amount)
    {
        HealingDone += amount;
        OnMetricChanged?.Invoke(MetricType.Healing, amount);
    }

    public void RecordWalking(float distance, float time)
    {
        DistanceWalked += distance;
        TimeWalked += time;
        OnMetricChanged?.Invoke(MetricType.Walking, distance);
    }

    public void RecordRunning(float distance, float time)
    {
        DistanceRun += distance;
        TimeRun += time;
        OnMetricChanged?.Invoke(MetricType.Running, distance);
    }

    public void RecordSneaking(float seconds)
    {
        TimeSneaking += seconds;
        OnMetricChanged?.Invoke(MetricType.Sneaking, seconds);
    }

    public void RecordNPCInteraction()
    {
        NPCInteractions++;
        OnMetricChanged?.Invoke(MetricType.NPCInteraction, 1);
    }

    public void RecordItemBought(int goldCost)
    {
        ItemsBought++;
        GoldSpent += goldCost;
        OnMetricChanged?.Invoke(MetricType.ItemBought, goldCost);
    }

    public void RecordItemSold(int goldEarned)
    {
        ItemsSold++;
        GoldEarned += goldEarned;
        OnMetricChanged?.Invoke(MetricType.ItemSold, goldEarned);
    }

    public void RecordLockPick(bool success)
    {
        LockPickAttempts++;
        if (success)
            LocksPickedSuccessfully++;
        OnMetricChanged?.Invoke(MetricType.LockPick, success ? 1 : 0);
    }

    public void RecordPotionUsed()
    {
        PotionsUsed++;
        OnMetricChanged?.Invoke(MetricType.PotionUsed, 1);
    }

    public void RecordScrollUsed()
    {
        ScrollsUsed++;
        OnMetricChanged?.Invoke(MetricType.ScrollUsed, 1);
    }
}
