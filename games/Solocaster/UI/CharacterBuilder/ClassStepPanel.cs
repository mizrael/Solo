using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solocaster.Character;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI.CharacterBuilder;

public class ClassStepPanel : Widget
{
    private readonly SelectableListWidget _classList;
    private readonly PanelWidget _detailPanel;
    private readonly LabelWidget _nameLabel;
    private readonly LabelWidget _descriptionLabel;
    private readonly LabelWidget _bonusesLabel;
    private readonly Action _onSelectionChanged;

    private readonly ClassTemplate[] _classes;
    private KeyboardState _previousKeyboardState;

    public ClassStepPanel(SpriteFont font, Vector2 size, Action onSelectionChanged)
    {
        _onSelectionChanged = onSelectionChanged;
        Size = size;

        _classes = CharacterTemplateLoader.GetAllClasses().ToArray();

        _classList = new SelectableListWidget
        {
            Items = _classes.Select(c => c.Name).ToList(),
            Font = font,
            Position = new Vector2(0, 0),
            Size = new Vector2(150, size.Y),
            BackgroundColor = UITheme.Panel.BackgroundColor,
            BorderColor = UITheme.Panel.BorderColor,
            BorderWidth = UITheme.Panel.BorderWidth
        };
        _classList.OnSelectionChanged += OnClassSelected;
        AddChild(_classList);

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

        var character = GameState.CurrentCharacter!;
        int index = Array.FindIndex(_classes, c => c.Id == character.ClassId);
        _classList.SelectedIndex = index >= 0 ? index : 0;
        if (_classList.SelectedIndex == 0 && _classes.Length > 0)
        {
            character.ClassId = _classes[0].Id;
        }
        UpdateDetails(_classList.SelectedIndex);
    }

    private void OnClassSelected(int index)
    {
        if (index >= 0 && index < _classes.Length)
        {
            GameState.CurrentCharacter!.ClassId = _classes[index].Id;
            UpdateDetails(index);
            _onSelectionChanged();
        }
    }

    private void UpdateDetails(int index)
    {
        if (index < 0 || index >= _classes.Length)
            return;

        var cls = _classes[index];
        _nameLabel.Text = cls.Name;
        _descriptionLabel.Text = cls.Description;

        var bonuses = cls.StatBonuses
            .Where(kv => kv.Value != 0)
            .Select(kv => $"{FormatStatName(kv.Key)}: {(kv.Value >= 0 ? "+" : "")}{kv.Value:0}")
            .ToList();

        _bonusesLabel.Text = bonuses.Count > 0
            ? "Stat Bonuses:\n" + string.Join("\n", bonuses)
            : "Stat Bonuses: None";
    }

    private static string FormatStatName(Stats stat)
    {
        return stat switch
        {
            Stats.Strength => "STR",
            Stats.Agility => "AGI",
            Stats.Vitality => "VIT",
            Stats.Intelligence => "INT",
            Stats.Wisdom => "WIS",
            _ => stat.ToString()
        };
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.Up))
            _classList.SelectPrevious();
        if (keyboardState.IsKeyDown(Keys.Down) && !_previousKeyboardState.IsKeyDown(Keys.Down))
            _classList.SelectNext();

        _previousKeyboardState = keyboardState;
    }
}
