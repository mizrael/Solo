using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solocaster.Character;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI.CharacterBuilder;

public class SexStepPanel : Widget
{
    private readonly ButtonWidget _maleButton;
    private readonly ButtonWidget _femaleButton;
    private readonly Action _onSelectionChanged;
    private KeyboardState _previousKeyboardState;

    private readonly Color _selectedColor = new Color(80, 70, 50);
    private readonly Color _normalColor;

    public SexStepPanel(SpriteFont font, Game game, Vector2 size, Action onSelectionChanged)
    {
        _onSelectionChanged = onSelectionChanged;
        Size = size;

        _normalColor = UITheme.Button.BackgroundColor;

        float buttonWidth = 150;
        float buttonHeight = 60;
        float spacing = 40;
        float totalWidth = buttonWidth * 2 + spacing;
        float startX = (size.X - totalWidth) / 2;
        float y = (size.Y - buttonHeight) / 2;

        _maleButton = new ButtonWidget
        {
            Text = "Male",
            Font = font,
            Position = new Vector2(startX, y),
            Size = new Vector2(buttonWidth, buttonHeight)
        };
        _maleButton.OnClick += () => SelectSex(Sex.Male);
        AddChild(_maleButton);

        _femaleButton = new ButtonWidget
        {
            Text = "Female",
            Font = font,
            Position = new Vector2(startX + buttonWidth + spacing, y),
            Size = new Vector2(buttonWidth, buttonHeight)
        };
        _femaleButton.OnClick += () => SelectSex(Sex.Female);
        AddChild(_femaleButton);

        UpdateButtonStates();
    }

    private void SelectSex(Sex sex)
    {
        GameState.CurrentCharacter!.Sex = sex;
        UpdateButtonStates();
        _onSelectionChanged();
    }

    private void UpdateButtonStates()
    {
        var currentSex = GameState.CurrentCharacter!.Sex;
        _maleButton.BackgroundColor = currentSex == Sex.Male ? _selectedColor : _normalColor;
        _femaleButton.BackgroundColor = currentSex == Sex.Female ? _selectedColor : _normalColor;
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        var keyboardState = Keyboard.GetState();

        if ((keyboardState.IsKeyDown(Keys.Left) && !_previousKeyboardState.IsKeyDown(Keys.Left)) ||
            (keyboardState.IsKeyDown(Keys.Right) && !_previousKeyboardState.IsKeyDown(Keys.Right)))
        {
            var currentSex = GameState.CurrentCharacter!.Sex;
            SelectSex(currentSex == Sex.Male ? Sex.Female : Sex.Male);
        }

        _previousKeyboardState = keyboardState;
    }
}
