using System.Collections.Generic;
using Solocaster.Inventory;

namespace Solocaster.Character;

public class ClassTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Starting stat bonuses (added to base stats)
    public Dictionary<StatType, float> StatBonuses { get; set; } = new();

    // Multiplier for how fast stat progress grows (1.0 = normal, 1.2 = 20% faster)
    public Dictionary<StatType, float> ProgressRates { get; set; } = new();

    // Multiplier for stat gain amount when progress reaches 100% (1.0 = normal)
    public Dictionary<StatType, float> GainMultipliers { get; set; } = new();
}
