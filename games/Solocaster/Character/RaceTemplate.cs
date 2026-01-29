using System.Collections.Generic;

namespace Solocaster.Character;

public class RaceTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Starting stat bonuses (added to base stats)
    public Dictionary<Stats, float> StatBonuses { get; set; } = new();

    // Multiplier for how fast stat progress grows (1.0 = normal, 1.2 = 20% faster)
    public Dictionary<Stats, float> ProgressRates { get; set; } = new();

    // Multiplier for stat gain amount when progress reaches 100% (1.0 = normal)
    public Dictionary<Stats, float> GainMultipliers { get; set; } = new();

    // How much stat progress each action grants (action → stat → multiplier of BaseProgressPerAction)
    public Dictionary<MetricType, Dictionary<Stats, float>> ActionProgress { get; set; } = new();

    // Multiplier for skill effectiveness (1.0 = normal, 1.2 = 20% better at the skill)
    public Dictionary<Skills, float> SkillEffectiveness { get; set; } = new();

    // Skill customization during character creation (for races like Human)
    public int SkillPoints { get; set; } = 0;        // Points to distribute (0 = skip step)
    public float PointBonus { get; set; } = 0.1f;    // Bonus per point (0.1 = +10%)
}
