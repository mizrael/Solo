using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solocaster.Character;
using Solocaster.Inventory;

namespace Solocaster.Components;

public class StatsComponent : Component
{
    private const float DefaultBaseStat = 10f;
    private const float BaseStatGain = 1f; // Amount gained when progress reaches 100%
    private const float BaseProgressPerAction = 5f; // Base % progress per action

    private const float StaminaDrainRate = 15f;
    private const float StaminaRegenDelay = 0.5f;
    private const float CombatActionRegenPause = 1.5f;
    private const float ExhaustedDuration = 2f;

    private readonly Dictionary<StatType, float> _baseStats = new();
    private readonly Dictionary<StatType, float> _equipmentBonuses = new();
    private readonly Dictionary<StatType, float> _statProgress = new(); // 0-100%

    private float _currentHealth;
    private float _currentMana;
    private float _currentStamina;
    private bool _isExhausted;
    private float _exhaustedTimer;
    private float _lastRunTime;
    private float _lastCombatActionTime;

    public string Name { get; set; } = "Adventurer";
    public RaceTemplate? Race { get; private set; }
    public ClassTemplate? Class { get; private set; }
    public Sex Sex { get; private set; } = Sex.Male;

    public PlayerMetrics Metrics { get; } = new();

    public float CurrentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = Math.Clamp(value, 0, GetTotalStat(StatType.MaxHealth));
            OnStatsChanged?.Invoke();
        }
    }

    public float CurrentMana
    {
        get => _currentMana;
        set
        {
            _currentMana = Math.Clamp(value, 0, GetTotalStat(StatType.MaxMana));
            OnStatsChanged?.Invoke();
        }
    }

    public float CurrentStamina
    {
        get => _currentStamina;
        set
        {
            _currentStamina = Math.Clamp(value, 0, MaxStamina);
            OnStatsChanged?.Invoke();
        }
    }

    public float MaxStamina => 50 + GetTotalStat(StatType.Vitality) * 5;
    public float StaminaRegenRate => 5 + GetTotalStat(StatType.Agility) * 0.5f;
    public bool IsExhausted => _isExhausted;

    public StatsComponent(GameObject owner) : base(owner)
    {
        InitializeBaseStats();
        InitializeStatProgress();

        Metrics.OnMetricChanged += OnMetricChanged;
    }

    private void InitializeBaseStats()
    {
        _baseStats[StatType.Strength] = DefaultBaseStat;
        _baseStats[StatType.Agility] = DefaultBaseStat;
        _baseStats[StatType.Vitality] = DefaultBaseStat;
        _baseStats[StatType.Intelligence] = DefaultBaseStat;
        _baseStats[StatType.Wisdom] = DefaultBaseStat;
    }

    private void InitializeStatProgress()
    {
        _statProgress[StatType.Strength] = 0f;
        _statProgress[StatType.Agility] = 0f;
        _statProgress[StatType.Vitality] = 0f;
        _statProgress[StatType.Intelligence] = 0f;
        _statProgress[StatType.Wisdom] = 0f;
    }

    public void SetCharacter(string raceId, string classId, Sex sex)
    {
        Race = CharacterTemplateLoader.GetRace(raceId);
        Class = CharacterTemplateLoader.GetClass(classId);
        Sex = sex;

        ApplyRaceAndClassBonuses();
        OnStatsChanged?.Invoke();
    }

    private void ApplyRaceAndClassBonuses()
    {
        // Reset to default and apply race/class bonuses
        InitializeBaseStats();

        if (Race != null)
        {
            foreach (var bonus in Race.StatBonuses)
            {
                if (_baseStats.ContainsKey(bonus.Key))
                    _baseStats[bonus.Key] += bonus.Value;
            }
        }

        if (Class != null)
        {
            foreach (var bonus in Class.StatBonuses)
            {
                if (_baseStats.ContainsKey(bonus.Key))
                    _baseStats[bonus.Key] += bonus.Value;
            }
        }

        // Ensure no stat goes below 1
        foreach (var stat in new[] { StatType.Strength, StatType.Agility, StatType.Vitality, StatType.Intelligence, StatType.Wisdom })
        {
            if (_baseStats[stat] < 1)
                _baseStats[stat] = 1;
        }
    }

    private void OnMetricChanged(MetricType metricType, float value)
    {
        if (Race == null || !Race.ActionProgress.TryGetValue(metricType, out var statProgress))
            return;

        // LockPick has special handling: failure gives only 20% of the progress
        float failureMultiplier = (metricType == MetricType.LockPick && value <= 0) ? 0.2f : 1f;

        foreach (var (stat, multiplier) in statProgress)
        {
            AddStatProgress(stat, BaseProgressPerAction * multiplier * failureMultiplier);
        }
    }

    public void AddStatProgress(StatType stat, float amount)
    {
        if (!_statProgress.ContainsKey(stat))
            return;

        // Apply race and class progress rate modifiers
        float progressRate = GetProgressRate(stat);
        float adjustedAmount = amount * progressRate;

        _statProgress[stat] += adjustedAmount;

        // Check for stat level up
        while (_statProgress[stat] >= 100f)
        {
            _statProgress[stat] -= 100f;
            LevelUpStat(stat);
        }

        OnStatProgressChanged?.Invoke(stat, _statProgress[stat]);
    }

    private float GetProgressRate(StatType stat)
    {
        float rate = 1f;

        if (Race != null && Race.ProgressRates.TryGetValue(stat, out var raceRate))
            rate *= raceRate;

        if (Class != null && Class.ProgressRates.TryGetValue(stat, out var classRate))
            rate *= classRate;

        return rate;
    }

    private float GetGainMultiplier(StatType stat)
    {
        float multiplier = 1f;

        if (Race != null && Race.GainMultipliers.TryGetValue(stat, out var raceMultiplier))
            multiplier *= raceMultiplier;

        if (Class != null && Class.GainMultipliers.TryGetValue(stat, out var classMultiplier))
            multiplier *= classMultiplier;

        return multiplier;
    }

    private void LevelUpStat(StatType stat)
    {
        float gainMultiplier = GetGainMultiplier(stat);
        float gain = BaseStatGain * gainMultiplier;

        _baseStats[stat] += gain;

        OnStatLevelUp?.Invoke(stat, _baseStats[stat]);
        OnStatsChanged?.Invoke();
    }

    public float GetStatProgress(StatType stat)
    {
        return _statProgress.TryGetValue(stat, out var progress) ? progress : 0f;
    }

    protected override void InitCore()
    {
        base.InitCore();
        // Initialize current values to max
        _currentHealth = GetTotalStat(StatType.MaxHealth);
        _currentMana = GetTotalStat(StatType.MaxMana);
        _currentStamina = MaxStamina;
    }

    public float GetBaseStat(StatType stat)
    {
        return _baseStats.TryGetValue(stat, out var value) ? value : 0f;
    }

    public void SetBaseStat(StatType stat, float value)
    {
        _baseStats[stat] = value;
        OnStatsChanged?.Invoke();
    }

    public float GetEquipmentBonus(StatType stat)
    {
        return _equipmentBonuses.TryGetValue(stat, out var value) ? value : 0f;
    }

    public float GetTotalStat(StatType stat)
    {
        return stat switch
        {
            StatType.MaxHealth => CalculateMaxHealth(),
            StatType.MaxWeight => CalculateMaxWeight(),
            StatType.MaxMana => CalculateMaxMana(),
            StatType.Damage => CalculateDamage(),
            StatType.Defense => CalculateDefense(),
            _ => GetBaseStat(stat) + GetEquipmentBonus(stat)
        };
    }

    private float CalculateMaxHealth()
    {
        float vitality = GetTotalStat(StatType.Vitality);
        float bonusHealth = GetEquipmentBonus(StatType.MaxHealth);
        // Base 50 HP + 5 per Vitality point
        return 50 + vitality * 5 + bonusHealth;
    }

    private float CalculateMaxWeight()
    {
        float strength = GetTotalStat(StatType.Strength);
        float bonusWeight = GetEquipmentBonus(StatType.MaxWeight);
        return 20 + strength * 2 + bonusWeight;
    }

    private float CalculateMaxMana()
    {
        float intelligence = GetTotalStat(StatType.Intelligence);
        float wisdom = GetTotalStat(StatType.Wisdom);
        float bonusMana = GetEquipmentBonus(StatType.MaxMana);
        // Base 20 MP + 3 per Intelligence + 2 per Wisdom
        return 20 + intelligence * 3 + wisdom * 2 + bonusMana;
    }

    private float CalculateDamage()
    {
        float strength = GetBaseStat(StatType.Strength) + GetEquipmentBonus(StatType.Strength);
        float bonusDamage = GetEquipmentBonus(StatType.Damage);
        return strength * 0.5f + bonusDamage;
    }

    private float CalculateDefense()
    {
        float agility = GetBaseStat(StatType.Agility) + GetEquipmentBonus(StatType.Agility);
        float bonusDefense = GetEquipmentBonus(StatType.Defense);
        return agility * 0.3f + bonusDefense;
    }

    public void OnItemEquipped(ItemInstance item)
    {
        foreach (var modifier in item.Template.StatModifiers)
        {
            if (!_equipmentBonuses.ContainsKey(modifier.Key))
                _equipmentBonuses[modifier.Key] = 0;
            _equipmentBonuses[modifier.Key] += modifier.Value;
        }
        OnStatsChanged?.Invoke();
    }

    public void OnItemUnequipped(ItemInstance item)
    {
        foreach (var modifier in item.Template.StatModifiers)
        {
            if (_equipmentBonuses.ContainsKey(modifier.Key))
            {
                _equipmentBonuses[modifier.Key] -= modifier.Value;
                if (_equipmentBonuses[modifier.Key] == 0)
                    _equipmentBonuses.Remove(modifier.Key);
            }
        }
        OnStatsChanged?.Invoke();
    }

    public bool MeetsRequirements(ItemTemplate template)
    {
        foreach (var requirement in template.Requirements)
        {
            if (GetBaseStat(requirement.Key) < requirement.Value)
                return false;
        }
        return true;
    }

    public void DrainStamina(float deltaTime)
    {
        CurrentStamina -= StaminaDrainRate * deltaTime;
        _lastRunTime = 0f; // Reset timer while running

        if (CurrentStamina <= 0)
        {
            CurrentStamina = 0;
            _isExhausted = true;
            _exhaustedTimer = ExhaustedDuration;
        }
    }

    public void UpdateStamina(float deltaTime, bool isRunning)
    {
        // Update exhausted state
        if (_isExhausted)
        {
            _exhaustedTimer -= deltaTime;
            if (_exhaustedTimer <= 0)
            {
                _isExhausted = false;
            }
            return; // No regen while exhausted
        }

        // Track time since last run
        if (!isRunning)
            _lastRunTime += deltaTime;

        // Track time since combat action
        _lastCombatActionTime += deltaTime;

        // Regenerate if conditions met
        bool canRegen = !isRunning
            && _lastRunTime >= StaminaRegenDelay
            && _lastCombatActionTime >= CombatActionRegenPause;

        if (canRegen && CurrentStamina < MaxStamina)
        {
            CurrentStamina += StaminaRegenRate * deltaTime;
        }
    }

    public void OnCombatAction()
    {
        _lastCombatActionTime = 0f;
    }

    public event Action? OnStatsChanged;
    public event Action<StatType, float>? OnStatProgressChanged;
    public event Action<StatType, float>? OnStatLevelUp;
}
