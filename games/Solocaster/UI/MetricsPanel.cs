using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solocaster.Character;
using Solocaster.Components;
using Solocaster.UI.Widgets;

namespace Solocaster.UI;

public class MetricsPanel : PanelWidget
{
    private const int PanelWidth = 300;
    private const int PanelHeight = 400;
    private const int LineHeight = 20;
    private const int SectionSpacing = 12;

    private readonly StatsComponent _stats;
    private readonly SpriteFont _font;

    // Stat progress rows
    private readonly StatProgressRowWidget _strengthRow;
    private readonly StatProgressRowWidget _agilityRow;
    private readonly StatProgressRowWidget _vitalityRow;
    private readonly StatProgressRowWidget _intelligenceRow;
    private readonly StatProgressRowWidget _wisdomRow;

    // Metric rows
    private readonly MetricRowWidget _meleeAttacksRow;
    private readonly MetricRowWidget _meleeDamageRow;
    private readonly MetricRowWidget _rangedAttacksRow;
    private readonly MetricRowWidget _rangedDamageRow;
    private readonly MetricRowWidget _damageTakenRow;
    private readonly MetricRowWidget _damageBlockedRow;
    private readonly MetricRowWidget _enemiesKilledRow;

    private readonly MetricRowWidget _spellsCastRow;
    private readonly MetricRowWidget _magicDamageRow;
    private readonly MetricRowWidget _healingDoneRow;
    private readonly MetricRowWidget _manaSpentRow;

    private readonly MetricRowWidget _distanceWalkedRow;
    private readonly MetricRowWidget _distanceRunRow;
    private readonly MetricRowWidget _timeSneakingRow;

    private readonly MetricRowWidget _npcInteractionsRow;
    private readonly MetricRowWidget _itemsBoughtRow;
    private readonly MetricRowWidget _itemsSoldRow;
    private readonly MetricRowWidget _goldSpentRow;
    private readonly MetricRowWidget _goldEarnedRow;
    private readonly MetricRowWidget _locksPickedRow;

    private readonly MetricRowWidget _potionsUsedRow;
    private readonly MetricRowWidget _scrollsUsedRow;

    public MetricsPanel(StatsComponent stats, SpriteFont font, Game game)
    {
        _stats = stats;
        _font = font;

        BackgroundColor = UITheme.Panel.BackgroundColor;
        BorderColor = UITheme.Panel.BorderColor;
        BorderWidth = UITheme.Panel.BorderWidth;
        ContentPadding = UITheme.Panel.ContentPadding;
        Scrollable = true;

        Size = new Vector2(PanelWidth, PanelHeight);
        Visible = false;

        int contentWidth = PanelWidth - ContentPadding * 2 - 6; // Account for scrollbar
        float y = 0;

        // Title
        var titleLabel = CreateLabel("Progress & Metrics", contentWidth, ref y, UITheme.Text.Highlight, true);
        AddChild(titleLabel);
        y += SectionSpacing;

        // Stat Progress Section
        var statHeader = CreateLabel("-- Stat Progress --", contentWidth, ref y, UITheme.Text.SectionHeader);
        AddChild(statHeader);
        y += 4;

        _strengthRow = CreateStatProgressRow("Strength", Stats.Strength, contentWidth, ref y);
        _agilityRow = CreateStatProgressRow("Agility", Stats.Agility, contentWidth, ref y);
        _vitalityRow = CreateStatProgressRow("Vitality", Stats.Vitality, contentWidth, ref y);
        _intelligenceRow = CreateStatProgressRow("Intelligence", Stats.Intelligence, contentWidth, ref y);
        _wisdomRow = CreateStatProgressRow("Wisdom", Stats.Wisdom, contentWidth, ref y);

        AddChild(_strengthRow);
        AddChild(_agilityRow);
        AddChild(_vitalityRow);
        AddChild(_intelligenceRow);
        AddChild(_wisdomRow);

        y += SectionSpacing;

        // Combat Section
        var combatHeader = CreateLabel("-- Combat --", contentWidth, ref y, UITheme.Text.SectionHeader);
        AddChild(combatHeader);
        y += 4;

        _meleeAttacksRow = CreateMetricRow("Melee Attacks", contentWidth, ref y);
        _meleeDamageRow = CreateMetricRow("Melee Damage", contentWidth, ref y);
        _rangedAttacksRow = CreateMetricRow("Ranged Attacks", contentWidth, ref y);
        _rangedDamageRow = CreateMetricRow("Ranged Damage", contentWidth, ref y);
        _damageTakenRow = CreateMetricRow("Damage Taken", contentWidth, ref y);
        _damageBlockedRow = CreateMetricRow("Damage Blocked", contentWidth, ref y);
        _enemiesKilledRow = CreateMetricRow("Enemies Killed", contentWidth, ref y);

        AddChild(_meleeAttacksRow);
        AddChild(_meleeDamageRow);
        AddChild(_rangedAttacksRow);
        AddChild(_rangedDamageRow);
        AddChild(_damageTakenRow);
        AddChild(_damageBlockedRow);
        AddChild(_enemiesKilledRow);

        y += SectionSpacing;

        // Magic Section
        var magicHeader = CreateLabel("-- Magic --", contentWidth, ref y, UITheme.Text.SectionHeader);
        AddChild(magicHeader);
        y += 4;

        _spellsCastRow = CreateMetricRow("Spells Cast", contentWidth, ref y);
        _magicDamageRow = CreateMetricRow("Magic Damage", contentWidth, ref y);
        _healingDoneRow = CreateMetricRow("Healing Done", contentWidth, ref y);
        _manaSpentRow = CreateMetricRow("Mana Spent", contentWidth, ref y);

        AddChild(_spellsCastRow);
        AddChild(_magicDamageRow);
        AddChild(_healingDoneRow);
        AddChild(_manaSpentRow);

        y += SectionSpacing;

        // Movement Section
        var movementHeader = CreateLabel("-- Movement --", contentWidth, ref y, UITheme.Text.SectionHeader);
        AddChild(movementHeader);
        y += 4;

        _distanceWalkedRow = CreateMetricRow("Distance Walked", contentWidth, ref y);
        _distanceRunRow = CreateMetricRow("Distance Run", contentWidth, ref y);
        _timeSneakingRow = CreateMetricRow("Time Sneaking", contentWidth, ref y);

        AddChild(_distanceWalkedRow);
        AddChild(_distanceRunRow);
        AddChild(_timeSneakingRow);

        y += SectionSpacing;

        // Social & Trade Section
        var socialHeader = CreateLabel("-- Social & Trade --", contentWidth, ref y, UITheme.Text.SectionHeader);
        AddChild(socialHeader);
        y += 4;

        _npcInteractionsRow = CreateMetricRow("NPC Interactions", contentWidth, ref y);
        _itemsBoughtRow = CreateMetricRow("Items Bought", contentWidth, ref y);
        _itemsSoldRow = CreateMetricRow("Items Sold", contentWidth, ref y);
        _goldSpentRow = CreateMetricRow("Gold Spent", contentWidth, ref y);
        _goldEarnedRow = CreateMetricRow("Gold Earned", contentWidth, ref y);
        _locksPickedRow = CreateMetricRow("Locks Picked", contentWidth, ref y);

        AddChild(_npcInteractionsRow);
        AddChild(_itemsBoughtRow);
        AddChild(_itemsSoldRow);
        AddChild(_goldSpentRow);
        AddChild(_goldEarnedRow);
        AddChild(_locksPickedRow);

        y += SectionSpacing;

        // Item Usage Section
        var itemHeader = CreateLabel("-- Item Usage --", contentWidth, ref y, UITheme.Text.SectionHeader);
        AddChild(itemHeader);
        y += 4;

        _potionsUsedRow = CreateMetricRow("Potions Used", contentWidth, ref y);
        _scrollsUsedRow = CreateMetricRow("Scrolls Used", contentWidth, ref y);

        AddChild(_potionsUsedRow);
        AddChild(_scrollsUsedRow);
    }

    private LabelWidget CreateLabel(string text, int width, ref float y, Color color, bool center = false)
    {
        var label = new LabelWidget
        {
            Text = text,
            Font = _font,
            TextColor = color,
            Position = new Vector2(0, y),
            Size = new Vector2(width, LineHeight),
            CenterHorizontally = center
        };
        y += LineHeight;
        return label;
    }

    private StatProgressRowWidget CreateStatProgressRow(string name, Stats statType, int width, ref float y)
    {
        var row = new StatProgressRowWidget
        {
            StatName = name,
            StatType = statType,
            Font = _font,
            Position = new Vector2(0, y),
            Size = new Vector2(width, LineHeight)
        };
        y += LineHeight;
        return row;
    }

    private MetricRowWidget CreateMetricRow(string label, int width, ref float y)
    {
        var row = new MetricRowWidget
        {
            Label = label,
            Font = _font,
            Position = new Vector2(0, y),
            Size = new Vector2(width, LineHeight)
        };
        y += LineHeight;
        return row;
    }

    public void CenterOnScreen(int screenWidth, int screenHeight)
    {
        Position = new Vector2(
            (screenWidth - Size.X) / 2,
            (screenHeight - Size.Y) / 2
        );
    }

    protected override void UpdateCore(GameTime gameTime, Microsoft.Xna.Framework.Input.MouseState mouseState, Microsoft.Xna.Framework.Input.MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        if (!Visible)
            return;

        // Update stat progress rows
        _strengthRow.StatValue = _stats.GetBaseStat(Stats.Strength);
        _strengthRow.Progress = _stats.GetStatProgress(Stats.Strength);

        _agilityRow.StatValue = _stats.GetBaseStat(Stats.Agility);
        _agilityRow.Progress = _stats.GetStatProgress(Stats.Agility);

        _vitalityRow.StatValue = _stats.GetBaseStat(Stats.Vitality);
        _vitalityRow.Progress = _stats.GetStatProgress(Stats.Vitality);

        _intelligenceRow.StatValue = _stats.GetBaseStat(Stats.Intelligence);
        _intelligenceRow.Progress = _stats.GetStatProgress(Stats.Intelligence);

        _wisdomRow.StatValue = _stats.GetBaseStat(Stats.Wisdom);
        _wisdomRow.Progress = _stats.GetStatProgress(Stats.Wisdom);

        // Update metric rows
        var metrics = _stats.Metrics;

        _meleeAttacksRow.Value = metrics.MeleeAttacks.ToString();
        _meleeDamageRow.Value = $"{metrics.MeleeDamageDealt:F0}";
        _rangedAttacksRow.Value = metrics.RangedAttacks.ToString();
        _rangedDamageRow.Value = $"{metrics.RangedDamageDealt:F0}";
        _damageTakenRow.Value = $"{metrics.DamageTaken:F0}";
        _damageBlockedRow.Value = $"{metrics.DamageBlocked:F0}";
        _enemiesKilledRow.Value = metrics.EnemiesKilled.ToString();

        _spellsCastRow.Value = metrics.SpellsCast.ToString();
        _magicDamageRow.Value = $"{metrics.MagicDamageDealt:F0}";
        _healingDoneRow.Value = $"{metrics.HealingDone:F0}";
        _manaSpentRow.Value = $"{metrics.ManaSpent:F0}";

        _distanceWalkedRow.Value = $"{metrics.DistanceWalked:F1}";
        _distanceRunRow.Value = $"{metrics.DistanceRun:F1}";
        _timeSneakingRow.Value = $"{metrics.TimeSneaking:F1}s";

        _npcInteractionsRow.Value = metrics.NPCInteractions.ToString();
        _itemsBoughtRow.Value = metrics.ItemsBought.ToString();
        _itemsSoldRow.Value = metrics.ItemsSold.ToString();
        _goldSpentRow.Value = metrics.GoldSpent.ToString();
        _goldEarnedRow.Value = metrics.GoldEarned.ToString();
        _locksPickedRow.Value = $"{metrics.LocksPickedSuccessfully}/{metrics.LockPickAttempts}";

        _potionsUsedRow.Value = metrics.PotionsUsed.ToString();
        _scrollsUsedRow.Value = metrics.ScrollsUsed.ToString();
    }
}
