using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets.Loaders;
using Solocaster.Character;
using Solocaster.Components;
using Solocaster.Inventory;
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
    private LabelWidget? _strengthLabel;
    private LabelWidget? _agilityLabel;
    private LabelWidget? _vitalityLabel;
    private LabelWidget? _intelligenceLabel;
    private LabelWidget? _wisdomLabel;
    private LabelWidget? _healthLabel;
    private LabelWidget? _manaLabel;
    private LabelWidget? _damageLabel;
    private LabelWidget? _defenseLabel;

    public StatsPanel(StatsComponent stats, SpriteFont font, Game game)
    {
        _stats = stats;
        _font = font;
        _game = game;

        ShowCloseButton = false;
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
        int width = 200;
        // Match InventoryPanel height: padding*2 + 40 + 4*(64+8) + 8 + 30 = 398
        int height = 398;

        Size = new Vector2(width, height);

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
            TextColor = new Color(220, 200, 160),
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
            TextColor = new Color(160, 160, 160),
            Position = new Vector2(padding, y),
            Size = new Vector2(width - padding * 2, lineHeight),
            CenterHorizontally = true
        };
        AddChild(_raceClassLabel);
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
        y += lineHeight;

        _wisdomLabel = CreateStatLabel(padding, y, width);
        AddChild(_wisdomLabel);
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
            TextColor = Color.LightGray,
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
