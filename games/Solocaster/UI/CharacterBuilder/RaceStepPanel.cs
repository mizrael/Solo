using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solocaster.Character;
using Solocaster.Inventory;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI.CharacterBuilder;

public class RaceStepPanel : Widget
{
    private readonly SelectableListWidget _raceList;
    private readonly PanelWidget _detailPanel;
    private readonly LabelWidget _nameLabel;
    private readonly LabelWidget _descriptionLabel;
    private readonly LabelWidget _bonusesLabel;
    private readonly Action _onSelectionChanged;

    private readonly RaceTemplate[] _races;
    private KeyboardState _previousKeyboardState;

    public RaceStepPanel(SpriteFont font, Vector2 size, Action onSelectionChanged)
    {
        _onSelectionChanged = onSelectionChanged;
        Size = size;

        _races = CharacterTemplateLoader.GetAllRaces().ToArray();

        // Race list on left
        _raceList = new SelectableListWidget
        {
            Items = _races.Select(r => r.Name).ToList(),
            Font = font,
            Position = new Vector2(0, 0),
            Size = new Vector2(150, size.Y),
            BackgroundColor = UITheme.Panel.BackgroundColor,
            BorderColor = UITheme.Panel.BorderColor,
            BorderWidth = UITheme.Panel.BorderWidth
        };
        _raceList.OnSelectionChanged += OnRaceSelected;
        AddChild(_raceList);

        // Detail panel on right
        _detailPanel = new PanelWidget
        {
            ShowCloseButton = false,
            BackgroundColor = UITheme.Panel.BackgroundColor,
            BorderColor = UITheme.Panel.BorderColor,
            BorderWidth = 2,
            ContentPadding = 0,
            Position = new Vector2(170, 0),
            Size = new Vector2(size.X - 170, size.Y)
        };
        AddChild(_detailPanel);

        int padding = 16;
        int y = padding;

        _nameLabel = new LabelWidget
        {
            Font = font,
            TextColor = UITheme.Text.Title,
            Position = new Vector2(padding, y),
            Size = new Vector2(_detailPanel.Size.X - padding * 2, 30)
        };
        _detailPanel.AddChild(_nameLabel);
        y += 35;

        _descriptionLabel = new LabelWidget
        {
            Font = font,
            TextColor = UITheme.Text.Secondary,
            Position = new Vector2(padding, y),
            Size = new Vector2(_detailPanel.Size.X - padding * 2, 80),
            WordWrap = true
        };
        _detailPanel.AddChild(_descriptionLabel);
        y += 90;

        _bonusesLabel = new LabelWidget
        {
            Font = font,
            TextColor = UITheme.Text.Primary,
            Position = new Vector2(padding, y),
            Size = new Vector2(_detailPanel.Size.X - padding * 2, 150)
        };
        _detailPanel.AddChild(_bonusesLabel);

        // Select current race or default to first
        var character = GameState.CurrentCharacter!;
        int index = Array.FindIndex(_races, r => r.Id == character.RaceId);
        _raceList.SelectedIndex = index >= 0 ? index : 0;
        if (_raceList.SelectedIndex == 0 && _races.Length > 0)
        {
            character.RaceId = _races[0].Id;
        }
        UpdateDetails(_raceList.SelectedIndex);
    }

    private void OnRaceSelected(int index)
    {
        if (index >= 0 && index < _races.Length)
        {
            GameState.CurrentCharacter!.RaceId = _races[index].Id;
            UpdateDetails(index);
            _onSelectionChanged();
        }
    }

    private void UpdateDetails(int index)
    {
        if (index < 0 || index >= _races.Length)
            return;

        var race = _races[index];
        _nameLabel.Text = race.Name;
        _descriptionLabel.Text = race.Description;

        var bonuses = race.StatBonuses
            .Where(kv => kv.Value != 0)
            .Select(kv => $"{FormatStatName(kv.Key)}: {(kv.Value >= 0 ? "+" : "")}{kv.Value:0}")
            .ToList();

        _bonusesLabel.Text = bonuses.Count > 0
            ? "Stat Bonuses:\n" + string.Join("\n", bonuses)
            : "Stat Bonuses: None";
    }

    private static string FormatStatName(StatType stat)
    {
        return stat switch
        {
            StatType.Strength => "STR",
            StatType.Agility => "AGI",
            StatType.Vitality => "VIT",
            StatType.Intelligence => "INT",
            StatType.Wisdom => "WIS",
            _ => stat.ToString()
        };
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.Up))
            _raceList.SelectPrevious();
        if (keyboardState.IsKeyDown(Keys.Down) && !_previousKeyboardState.IsKeyDown(Keys.Down))
            _raceList.SelectNext();

        _previousKeyboardState = keyboardState;
    }
}
