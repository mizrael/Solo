using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solocaster.Inventory;

namespace Solocaster.Components;

public class StatsComponent : Component
{
    private readonly Dictionary<StatType, float> _baseStats = new();
    private readonly Dictionary<StatType, float> _equipmentBonuses = new();

    public StatsComponent(GameObject owner) : base(owner)
    {
        _baseStats[StatType.Strength] = 10;
        _baseStats[StatType.Agility] = 10;
        _baseStats[StatType.Vitality] = 10;
        _baseStats[StatType.Intelligence] = 10;
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
            _ => GetBaseStat(stat) + GetEquipmentBonus(stat)
        };
    }

    private float CalculateMaxHealth()
    {
        float vitality = GetTotalStat(StatType.Vitality);
        float bonusHealth = GetEquipmentBonus(StatType.MaxHealth);
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
        float bonusMana = GetEquipmentBonus(StatType.MaxMana);
        return 20 + intelligence * 3 + bonusMana;
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

    public event Action? OnStatsChanged;
}
