using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solocaster.Components;
using Solocaster.Inventory;
using Solocaster.UI.Widgets;

namespace Solocaster.UI;

public class StatsPanel : PanelWidget
{
    private readonly StatsComponent _stats;
    private readonly SpriteFont _font;

    private LabelWidget? _titleLabel;
    private LabelWidget? _strengthLabel;
    private LabelWidget? _agilityLabel;
    private LabelWidget? _vitalityLabel;
    private LabelWidget? _intelligenceLabel;
    private LabelWidget? _healthLabel;
    private LabelWidget? _manaLabel;
    private LabelWidget? _damageLabel;
    private LabelWidget? _defenseLabel;

    public StatsPanel(StatsComponent stats, SpriteFont font)
    {
        _stats = stats;
        _font = font;

        BackgroundColor = new Color(20, 20, 25, 240);
        BorderColor = new Color(100, 80, 60);
        BorderWidth = 3;

        BuildLayout();

        _stats.OnStatsChanged += RefreshStats;
    }

    private void BuildLayout()
    {
        int padding = 16;
        int lineHeight = 24;
        int width = 180;
        int contentLines = 10; // title + 4 primary + separator + 4 derived
        int height = padding * 2 + lineHeight * contentLines;

        Size = new Vector2(width, height);

        int y = padding;

        // Title
        _titleLabel = new LabelWidget
        {
            Text = "Character",
            Font = _font,
            TextColor = new Color(200, 180, 140),
            Position = new Vector2(padding, y),
            Size = new Vector2(width - padding * 2, lineHeight),
            CenterHorizontally = true
        };
        AddChild(_titleLabel);
        y += lineHeight + 8;

        // Primary stats
        _strengthLabel = CreateStatLabel(padding, y, width);
        AddChild(_strengthLabel);
        y += lineHeight;

        _agilityLabel = CreateStatLabel(padding, y, width);
        AddChild(_agilityLabel);
        y += lineHeight;

        _vitalityLabel = CreateStatLabel(padding, y, width);
        AddChild(_vitalityLabel);
        y += lineHeight;

        _intelligenceLabel = CreateStatLabel(padding, y, width);
        AddChild(_intelligenceLabel);
        y += lineHeight + 8;

        // Derived stats
        _healthLabel = CreateStatLabel(padding, y, width);
        AddChild(_healthLabel);
        y += lineHeight;

        _manaLabel = CreateStatLabel(padding, y, width);
        AddChild(_manaLabel);
        y += lineHeight;

        _damageLabel = CreateStatLabel(padding, y, width);
        AddChild(_damageLabel);
        y += lineHeight;

        _defenseLabel = CreateStatLabel(padding, y, width);
        AddChild(_defenseLabel);

        RefreshStats();
    }

    private LabelWidget CreateStatLabel(int x, int y, int width)
    {
        return new LabelWidget
        {
            Font = _font,
            TextColor = Color.LightGray,
            Position = new Vector2(x, y),
            Size = new Vector2(width - x * 2, 20)
        };
    }

    private void RefreshStats()
    {
        if (_strengthLabel != null)
            _strengthLabel.Text = FormatStat("STR", StatType.Strength);

        if (_agilityLabel != null)
            _agilityLabel.Text = FormatStat("AGI", StatType.Agility);

        if (_vitalityLabel != null)
            _vitalityLabel.Text = FormatStat("VIT", StatType.Vitality);

        if (_intelligenceLabel != null)
            _intelligenceLabel.Text = FormatStat("INT", StatType.Intelligence);

        if (_healthLabel != null)
            _healthLabel.Text = $"Health: {_stats.GetTotalStat(StatType.MaxHealth):0}";

        if (_manaLabel != null)
            _manaLabel.Text = $"Mana: {_stats.GetTotalStat(StatType.MaxMana):0}";

        if (_damageLabel != null)
        {
            var damage = _stats.GetTotalStat(StatType.Damage);
            _damageLabel.Text = damage > 0 ? $"Damage: +{damage:0}" : "Damage: 0";
        }

        if (_defenseLabel != null)
        {
            var defense = _stats.GetTotalStat(StatType.Defense);
            _defenseLabel.Text = defense > 0 ? $"Defense: +{defense:0}" : "Defense: 0";
        }
    }

    private string FormatStat(string label, StatType stat)
    {
        var baseStat = _stats.GetBaseStat(stat);
        var bonus = _stats.GetEquipmentBonus(stat);

        if (bonus > 0)
            return $"{label}: {baseStat:0} (+{bonus:0})";
        if (bonus < 0)
            return $"{label}: {baseStat:0} ({bonus:0})";

        return $"{label}: {baseStat:0}";
    }
}
