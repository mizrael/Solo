using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solocaster.Character;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI.CharacterBuilder;

public class SkillsStepPanel : Widget
{
    private readonly LabelWidget _headerLabel;
    private readonly List<PointDistributorWidget> _skillRows = new();
    private readonly Action _onSelectionChanged;

    private readonly int _totalPoints;
    private readonly float _pointBonus;
    private int _selectedRowIndex;
    private KeyboardState _previousKeyboardState;

    public SkillsStepPanel(SpriteFont font, Vector2 size, Action onSelectionChanged, int totalPoints, float pointBonus)
    {
        _onSelectionChanged = onSelectionChanged;
        _totalPoints = totalPoints;
        _pointBonus = pointBonus;
        Size = size;

        _headerLabel = new LabelWidget
        {
            Text = $"Distribute Skill Points ({totalPoints} remaining)",
            Font = font,
            TextColor = UITheme.Text.Title,
            Position = new Vector2(0, 0),
            Size = new Vector2(size.X, 30),
            CenterHorizontally = true
        };
        AddChild(_headerLabel);

        var skills = Enum.GetValues<Skills>();
        int y = 50;

        foreach (var skill in skills)
        {
            var row = new PointDistributorWidget(skill.ToString(), font, pointBonus)
            {
                Position = new Vector2((size.X - 400) / 2, y)
            };
            row.OnValueChanged += OnSkillValueChanged;
            _skillRows.Add(row);
            AddChild(row);
            y += 45;
        }

        // Load existing allocations from character data
        var character = GameState.CurrentCharacter!;
        for (int i = 0; i < _skillRows.Count; i++)
        {
            var skill = skills[i];
            if (character.SkillPointAllocations.TryGetValue(skill, out int points))
            {
                _skillRows[i].Value = points;
            }
        }

        UpdateButtonStates();
        UpdateHighlight();
    }

    private void OnSkillValueChanged()
    {
        SaveAllocations();
        UpdateButtonStates();
        _onSelectionChanged();
    }

    private void SaveAllocations()
    {
        var character = GameState.CurrentCharacter!;
        character.SkillPointAllocations.Clear();

        var skills = Enum.GetValues<Skills>();
        for (int i = 0; i < _skillRows.Count; i++)
        {
            if (_skillRows[i].Value > 0)
            {
                character.SkillPointAllocations[skills[i]] = _skillRows[i].Value;
            }
        }
    }

    private void UpdateButtonStates()
    {
        int usedPoints = 0;
        foreach (var row in _skillRows)
        {
            usedPoints += row.Value;
        }

        int remaining = _totalPoints - usedPoints;
        _headerLabel.Text = $"Distribute Skill Points ({remaining} remaining)";

        foreach (var row in _skillRows)
        {
            row.CanIncrease = remaining > 0;
        }
    }

    private void UpdateHighlight()
    {
        for (int i = 0; i < _skillRows.Count; i++)
        {
            // Visual highlight for selected row could be added here if needed
        }
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        var keyboardState = Keyboard.GetState();

        bool upPressed = keyboardState.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.Up);
        bool downPressed = keyboardState.IsKeyDown(Keys.Down) && !_previousKeyboardState.IsKeyDown(Keys.Down);
        bool leftPressed = keyboardState.IsKeyDown(Keys.Left) && !_previousKeyboardState.IsKeyDown(Keys.Left);
        bool rightPressed = keyboardState.IsKeyDown(Keys.Right) && !_previousKeyboardState.IsKeyDown(Keys.Right);

        if (upPressed && _selectedRowIndex > 0)
        {
            _selectedRowIndex--;
            UpdateHighlight();
        }
        else if (downPressed && _selectedRowIndex < _skillRows.Count - 1)
        {
            _selectedRowIndex++;
            UpdateHighlight();
        }

        if (leftPressed || rightPressed)
        {
            _skillRows[_selectedRowIndex].HandleKeyboardInput(leftPressed, rightPressed);
        }

        _previousKeyboardState = keyboardState;
    }
}
