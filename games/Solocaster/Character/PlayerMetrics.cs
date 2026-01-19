using System;

namespace Solocaster.Character;

public class PlayerMetrics
{
    public static class Metric
    {
        public const string MeleeAttack = nameof(MeleeAttack);
        public const string RangedAttack = nameof(RangedAttack);
        public const string DamageTaken = nameof(DamageTaken);
        public const string DamageBlocked = nameof(DamageBlocked);
        public const string EnemyKilled = nameof(EnemyKilled);
        public const string SpellCast = nameof(SpellCast);
        public const string MagicDamage = nameof(MagicDamage);
        public const string Healing = nameof(Healing);
        public const string Walking = nameof(Walking);
        public const string Running = nameof(Running);
        public const string Hiding = nameof(Hiding);
        public const string Sneaking = nameof(Sneaking);
        public const string NPCInteraction = nameof(NPCInteraction);
        public const string ItemBought = nameof(ItemBought);
        public const string ItemSold = nameof(ItemSold);
        public const string LockPick = nameof(LockPick);
        public const string PotionUsed = nameof(PotionUsed);
        public const string ScrollUsed = nameof(ScrollUsed);
    }

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
    public float DistanceRun { get; private set; }
    public float TimeHiding { get; private set; }
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

    // Events for when metrics change (so stat progress can update)
    public event Action<string, float>? OnMetricChanged;

    public void RecordMeleeAttack(float damage)
    {
        MeleeAttacks++;
        MeleeDamageDealt += damage;
        OnMetricChanged?.Invoke(Metric.MeleeAttack, damage);
    }

    public void RecordRangedAttack(float damage)
    {
        RangedAttacks++;
        RangedDamageDealt += damage;
        OnMetricChanged?.Invoke(Metric.RangedAttack, damage);
    }

    public void RecordDamageTaken(float damage)
    {
        DamageTaken += damage;
        OnMetricChanged?.Invoke(Metric.DamageTaken, damage);
    }

    public void RecordDamageBlocked(float damage)
    {
        DamageBlocked += damage;
        OnMetricChanged?.Invoke(Metric.DamageBlocked, damage);
    }

    public void RecordEnemyKilled()
    {
        EnemiesKilled++;
        OnMetricChanged?.Invoke(Metric.EnemyKilled, 1);
    }

    public void RecordSpellCast(float manaCost)
    {
        SpellsCast++;
        ManaSpent += manaCost;
        OnMetricChanged?.Invoke(Metric.SpellCast, manaCost);
    }

    public void RecordMagicDamage(float damage)
    {
        MagicDamageDealt += damage;
        OnMetricChanged?.Invoke(Metric.MagicDamage, damage);
    }

    public void RecordHealing(float amount)
    {
        HealingDone += amount;
        OnMetricChanged?.Invoke(Metric.Healing, amount);
    }

    public void RecordWalking(float distance)
    {
        DistanceWalked += distance;
        OnMetricChanged?.Invoke(Metric.Walking, distance);
    }

    public void RecordRunning(float distance)
    {
        DistanceRun += distance;
        OnMetricChanged?.Invoke(Metric.Running, distance);
    }

    public void RecordHiding(float seconds)
    {
        TimeHiding += seconds;
        OnMetricChanged?.Invoke(Metric.Hiding, seconds);
    }

    public void RecordSneaking(float seconds)
    {
        TimeSneaking += seconds;
        OnMetricChanged?.Invoke(Metric.Sneaking, seconds);
    }

    public void RecordNPCInteraction()
    {
        NPCInteractions++;
        OnMetricChanged?.Invoke(Metric.NPCInteraction, 1);
    }

    public void RecordItemBought(int goldCost)
    {
        ItemsBought++;
        GoldSpent += goldCost;
        OnMetricChanged?.Invoke(Metric.ItemBought, goldCost);
    }

    public void RecordItemSold(int goldEarned)
    {
        ItemsSold++;
        GoldEarned += goldEarned;
        OnMetricChanged?.Invoke(Metric.ItemSold, goldEarned);
    }

    public void RecordLockPick(bool success)
    {
        LockPickAttempts++;
        if (success)
            LocksPickedSuccessfully++;
        OnMetricChanged?.Invoke(Metric.LockPick, success ? 1 : 0);
    }

    public void RecordPotionUsed()
    {
        PotionsUsed++;
        OnMetricChanged?.Invoke(Metric.PotionUsed, 1);
    }

    public void RecordScrollUsed()
    {
        ScrollsUsed++;
        OnMetricChanged?.Invoke(Metric.ScrollUsed, 1);
    }
}
