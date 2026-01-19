using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solocaster.State;
using Solocaster.UI.Widgets;
using Solocaster.Utilities;

namespace Solocaster.UI.CharacterBuilder;

public class NameStepPanel : Widget
{
    private readonly TextInputWidget _nameInput;
    private readonly ButtonWidget _randomButton;
    private readonly Action _onSelectionChanged;

    public NameStepPanel(SpriteFont font, Vector2 size, Action onSelectionChanged)
    {
        _onSelectionChanged = onSelectionChanged;
        Size = size;

        float inputWidth = 300;
        float inputHeight = 40;
        float buttonWidth = 100;
        float spacing = 20;
        float totalWidth = inputWidth + spacing + buttonWidth;
        float startX = (size.X - totalWidth) / 2;
        float y = (size.Y - inputHeight) / 2;

        var labelY = y - 40;
        var label = new LabelWidget
        {
            Text = "Choose your name:",
            Font = font,
            TextColor = new Color(200, 180, 140),
            Position = new Vector2(0, labelY),
            Size = new Vector2(size.X, 30),
            CenterHorizontally = true
        };
        AddChild(label);

        _nameInput = new TextInputWidget
        {
            Font = font,
            Text = GameState.CurrentCharacter!.Name,
            MaxLength = 20,
            PlaceholderText = "Enter your name...",
            Position = new Vector2(startX, y),
            Size = new Vector2(inputWidth, inputHeight)
        };
        _nameInput.OnTextChanged += OnNameChanged;
        AddChild(_nameInput);

        _randomButton = new ButtonWidget
        {
            Text = "Random",
            Font = font,
            Position = new Vector2(startX + inputWidth + spacing, y),
            Size = new Vector2(buttonWidth, inputHeight)
        };
        _randomButton.OnClick += OnRandomClicked;
        AddChild(_randomButton);
    }

    private void OnNameChanged(string name)
    {
        GameState.CurrentCharacter!.Name = name;
        _onSelectionChanged();
    }

    private void OnRandomClicked()
    {
        var character = GameState.CurrentCharacter!;
        var name = NameGenerator.Generate(character.Sex);
        _nameInput.Text = name;
        character.Name = name;
        _onSelectionChanged();
    }
}
