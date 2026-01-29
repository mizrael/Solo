using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets.Loaders;
using Solocaster.Character;
using Solocaster.Components;
using Solocaster.Inventory;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI;

public class StatsPanel : PanelWidget
{
    private const int AvatarSize = 64;

    private readonly StatsComponent _stats;
    private readonly SpriteFont _font;
    private readonly Game _game;

    private ImageWidget? _avatarImage;
    private LabelWidget? _nameLabel;
    private LabelWidget? _raceClassLabel;

    // Primary stats
    private LabelWidget? _strengthLabel;
    private LabelWidget? _agilityLabel;
    private LabelWidget? _vitalityLabel;
    private LabelWidget? _intelligenceLabel;
    private LabelWidget? _wisdomLabel;

    // Combat stats
    private LabelWidget? _damageLabel;
    private LabelWidget? _defenseLabel;
    private LabelWidget? _attackSpeedLabel;
    private LabelWidget? _critChanceLabel;

    // Magic stats
    private LabelWidget? _manaLabel;
    private LabelWidget? _manaRegenLabel;
    private LabelWidget? _spellPowerLabel;

    // Resources
    private LabelWidget? _healthLabel;
    private LabelWidget? _maxWeightLabel;

    public StatsPanel(StatsComponent stats, SpriteFont font, Game game)
    {
        _stats = stats;
        _font = font;
        _game = game;

        ShowCloseButton = false;
        BackgroundColor = UITheme.Panel.BackgroundColor;
        BorderColor = UITheme.Panel.BorderColor;
        BorderWidth = UITheme.Panel.BorderWidth;
        ContentPadding = 0; // StatsPanel handles its own padding

        BuildLayout();

        _stats.OnStatsChanged += RefreshStats;
    }

    private void BuildLayout()
    {
        int padding = 16;
        int lineHeight = 22;
        int sectionHeaderHeight = 20;
        int sectionSpacing = 12;
        int width = 200;

        int y = padding;

        // Avatar
        _avatarImage = new ImageWidget
        {
            Position = new Vector2((width - AvatarSize) / 2, y),
            Size = new Vector2(AvatarSize, AvatarSize),
            ScaleToFit = true
        };
        AddChild(_avatarImage);
        LoadAvatar();

        y += AvatarSize + 4;

        // Name
        _nameLabel = new LabelWidget
        {
            Text = _stats.Name,
            Font = _font,
            TextColor = UITheme.Text.Title,
            Position = new Vector2(padding, y),
            Size = new Vector2(width - padding * 2, lineHeight),
            CenterHorizontally = true
        };
        AddChild(_nameLabel);
        y += lineHeight;

        // Race and Class
        _raceClassLabel = new LabelWidget
        {
            Text = GetRaceClassName(),
            Font = _font,
            TextColor = UITheme.Text.Subtitle,
            Position = new Vector2(padding, y),
            Size = new Vector2(width - padding * 2, lineHeight),
            CenterHorizontally = true
        };
        AddChild(_raceClassLabel);
        y += lineHeight + sectionSpacing;

        // === PRIMARY STATS ===
        AddChild(CreateSectionHeader("Primary", padding, y, width));
        y += sectionHeaderHeight;

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
        y += lineHeight;

        _wisdomLabel = CreateStatLabel(padding, y, width);
        AddChild(_wisdomLabel);
        y += lineHeight + sectionSpacing;

        // === COMBAT ===
        AddChild(CreateSectionHeader("Combat", padding, y, width));
        y += sectionHeaderHeight;

        _damageLabel = CreateStatLabel(padding, y, width);
        AddChild(_damageLabel);
        y += lineHeight;

        _defenseLabel = CreateStatLabel(padding, y, width);
        AddChild(_defenseLabel);
        y += lineHeight;

        _attackSpeedLabel = CreateStatLabel(padding, y, width);
        AddChild(_attackSpeedLabel);
        y += lineHeight;

        _critChanceLabel = CreateStatLabel(padding, y, width);
        AddChild(_critChanceLabel);
        y += lineHeight + sectionSpacing;

        // === MAGIC ===
        AddChild(CreateSectionHeader("Magic", padding, y, width));
        y += sectionHeaderHeight;

        _manaLabel = CreateStatLabel(padding, y, width);
        AddChild(_manaLabel);
        y += lineHeight;

        _manaRegenLabel = CreateStatLabel(padding, y, width);
        AddChild(_manaRegenLabel);
        y += lineHeight;

        _spellPowerLabel = CreateStatLabel(padding, y, width);
        AddChild(_spellPowerLabel);
        y += lineHeight + sectionSpacing;

        // === RESOURCES ===
        AddChild(CreateSectionHeader("Resources", padding, y, width));
        y += sectionHeaderHeight;

        _healthLabel = CreateStatLabel(padding, y, width);
        AddChild(_healthLabel);
        y += lineHeight;

        _maxWeightLabel = CreateStatLabel(padding, y, width);
        AddChild(_maxWeightLabel);
        y += lineHeight;

        // Set final panel size
        int height = y + padding;
        Size = new Vector2(width, height);

        RefreshStats();
    }

    private LabelWidget CreateSectionHeader(string text, int x, int y, int width)
    {
        return new LabelWidget
        {
            Text = text,
            Font = _font,
            TextColor = UITheme.Text.Muted,
            Position = new Vector2(x, y),
            Size = new Vector2(width - x * 2, 20)
        };
    }

    private void LoadAvatar()
    {
        if (_avatarImage == null)
            return;

        try
        {
            var spriteSheet = SpriteSheetLoader.Get("avatars", _game);
            var spriteName = GetAvatarSpriteName();
            var sprite = spriteSheet.Get(spriteName);
            _avatarImage.Texture = sprite.Texture;
            _avatarImage.SourceRectangle = sprite.Bounds;
        }
        catch
        {
            // Avatar not found, will show empty
        }
    }

    private string GetAvatarSpriteName()
    {
        // Use the avatar selected in character creation
        var avatarFromState = GameState.CurrentCharacter?.AvatarSpriteName;
        if (!string.IsNullOrEmpty(avatarFromState))
            return avatarFromState;

        // Fallback to constructing from race/class/sex
        var race = _stats.Race?.Id ?? "human";
        var cls = _stats.Class?.Id ?? "warrior";
        var sex = _stats.Sex == Sex.Female ? "female" : "male";
        return $"{race}_{cls}_{sex}";
    }

    private string GetRaceClassName()
    {
        var raceName = _stats.Race?.Name ?? "Human";
        var className = _stats.Class?.Name ?? "Warrior";
        return $"{raceName} {className}";
    }

    private LabelWidget CreateStatLabel(int x, int y, int width)
    {
        return new LabelWidget
        {
            Font = _font,
            TextColor = UITheme.Text.Secondary,
            Position = new Vector2(x, y),
            Size = new Vector2(width - x * 2, 20)
        };
    }

    private void RefreshStats()
    {
        if (_nameLabel != null)
            _nameLabel.Text = _stats.Name;

        if (_raceClassLabel != null)
            _raceClassLabel.Text = GetRaceClassName();

        // Primary stats
        if (_strengthLabel != null)
            _strengthLabel.Text = FormatStat("STR", StatType.Strength);

        if (_agilityLabel != null)
            _agilityLabel.Text = FormatStat("AGI", StatType.Agility);

        if (_vitalityLabel != null)
            _vitalityLabel.Text = FormatStat("VIT", StatType.Vitality);

        if (_intelligenceLabel != null)
            _intelligenceLabel.Text = FormatStat("INT", StatType.Intelligence);

        if (_wisdomLabel != null)
            _wisdomLabel.Text = FormatStat("WIS", StatType.Wisdom);

        // Combat stats
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

        if (_attackSpeedLabel != null)
        {
            var attackSpeed = _stats.GetTotalStat(StatType.AttackSpeed);
            _attackSpeedLabel.Text = $"Atk Speed: {attackSpeed:0.0}";
        }

        if (_critChanceLabel != null)
        {
            var critChance = _stats.GetTotalStat(StatType.CriticalChance);
            _critChanceLabel.Text = $"Crit: {critChance:0}%";
        }

        // Magic stats
        if (_manaLabel != null)
            _manaLabel.Text = $"Mana: {_stats.GetTotalStat(StatType.MaxMana):0}";

        if (_manaRegenLabel != null)
        {
            var manaRegen = _stats.GetTotalStat(StatType.ManaRegen);
            _manaRegenLabel.Text = $"Mana Regen: {manaRegen:0.0}/s";
        }

        if (_spellPowerLabel != null)
        {
            var spellPower = _stats.GetTotalStat(StatType.SpellPower);
            _spellPowerLabel.Text = spellPower > 0 ? $"Spell Power: +{spellPower:0}" : "Spell Power: 0";
        }

        // Resources
        if (_healthLabel != null)
            _healthLabel.Text = $"Health: {_stats.GetTotalStat(StatType.MaxHealth):0}";

        if (_maxWeightLabel != null)
            _maxWeightLabel.Text = $"Max Weight: {_stats.GetTotalStat(StatType.MaxWeight):0}";

        // Reload avatar in case race/class/sex changed
        LoadAvatar();
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
