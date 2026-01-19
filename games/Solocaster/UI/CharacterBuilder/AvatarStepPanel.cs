using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo.Assets.Loaders;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI.CharacterBuilder;

public class AvatarStepPanel : Widget
{
    private readonly SpriteFont _font;
    private readonly Game _game;
    private readonly Action _onSelectionChanged;
    private readonly List<ImageWidget> _avatarWidgets = new();
    private readonly List<string> _avatarNames = new();
    private int _selectedIndex;
    private int _hoveredIndex = -1;
    private KeyboardState _previousKeyboardState;

    private const int AvatarSize = 80;
    private const int AvatarPadding = 10;
    private const int Columns = 4;

    private static Texture2D? _pixelTexture;

    public AvatarStepPanel(SpriteFont font, Game game, Vector2 size, Action onSelectionChanged)
    {
        _font = font;
        _game = game;
        _onSelectionChanged = onSelectionChanged;
        Size = size;

        LoadAvatars();
    }

    private static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
    {
        if (_pixelTexture == null)
        {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        return _pixelTexture;
    }

    private void LoadAvatars()
    {
        // Clear previous
        foreach (var widget in _avatarWidgets)
            RemoveChild(widget);
        _avatarWidgets.Clear();
        _avatarNames.Clear();

        var character = GameState.CurrentCharacter!;

        try
        {
            var spriteSheet = SpriteSheetLoader.Get("avatars", _game);

            // Show all avatars - player has full freedom to choose
            var allSprites = spriteSheet.Sprites
                .Select(s => s.Name)
                .ToList();

            float startY = 20;
            float totalWidth = Columns * (AvatarSize + AvatarPadding) - AvatarPadding;
            float startX = (Size.X - totalWidth) / 2;

            for (int i = 0; i < allSprites.Count; i++)
            {
                var spriteName = allSprites[i];
                var sprite = spriteSheet.Get(spriteName);

                int col = i % Columns;
                int row = i / Columns;

                var avatarWidget = new ImageWidget
                {
                    Texture = sprite.Texture,
                    SourceRectangle = sprite.Bounds,
                    ScaleToFit = true,
                    Position = new Vector2(
                        startX + col * (AvatarSize + AvatarPadding),
                        startY + row * (AvatarSize + AvatarPadding)
                    ),
                    Size = new Vector2(AvatarSize, AvatarSize)
                };

                _avatarWidgets.Add(avatarWidget);
                _avatarNames.Add(spriteName);
                AddChild(avatarWidget);
            }

            // Select current avatar or first one
            _selectedIndex = _avatarNames.IndexOf(character.AvatarSpriteName);
            if (_selectedIndex < 0 && _avatarNames.Count > 0)
            {
                _selectedIndex = 0;
                character.AvatarSpriteName = _avatarNames[0];
                _onSelectionChanged();
            }
        }
        catch
        {
            // Handle missing spritesheet gracefully
            var errorLabel = new LabelWidget
            {
                Text = "Avatar spritesheet not found",
                Font = _font,
                TextColor = Color.Red,
                Position = new Vector2(0, Size.Y / 2 - 15),
                Size = new Vector2(Size.X, 30),
                CenterHorizontally = true
            };
            AddChild(errorLabel);
        }
    }

    private void SelectAvatar(int index)
    {
        if (index >= 0 && index < _avatarNames.Count)
        {
            _selectedIndex = index;
            GameState.CurrentCharacter!.AvatarSpriteName = _avatarNames[index];
            _onSelectionChanged();
        }
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        var mousePoint = new Point(mouseState.X, mouseState.Y);

        // Track hover state
        _hoveredIndex = -1;
        for (int i = 0; i < _avatarWidgets.Count; i++)
        {
            if (_avatarWidgets[i].Bounds.Contains(mousePoint))
            {
                _hoveredIndex = i;
                break;
            }
        }

        // Handle click on avatar
        if (mouseState.LeftButton == ButtonState.Pressed &&
            previousMouseState.LeftButton == ButtonState.Released &&
            _hoveredIndex >= 0)
        {
            SelectAvatar(_hoveredIndex);
        }

        // Keyboard navigation
        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Left) && !_previousKeyboardState.IsKeyDown(Keys.Left))
            SelectAvatar(Math.Max(0, _selectedIndex - 1));
        if (keyboardState.IsKeyDown(Keys.Right) && !_previousKeyboardState.IsKeyDown(Keys.Right))
            SelectAvatar(Math.Min(_avatarNames.Count - 1, _selectedIndex + 1));
        if (keyboardState.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.Up))
            SelectAvatar(Math.Max(0, _selectedIndex - Columns));
        if (keyboardState.IsKeyDown(Keys.Down) && !_previousKeyboardState.IsKeyDown(Keys.Down))
            SelectAvatar(Math.Min(_avatarNames.Count - 1, _selectedIndex + Columns));

        _previousKeyboardState = keyboardState;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        base.RenderCore(spriteBatch);

        var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);

        // Draw hover highlight (if not the selected one)
        if (_hoveredIndex >= 0 && _hoveredIndex < _avatarWidgets.Count && _hoveredIndex != _selectedIndex)
        {
            var hovered = _avatarWidgets[_hoveredIndex];
            var bounds = hovered.Bounds;
            bounds.Inflate(3, 3);

            var hoverColor = new Color(150, 150, 150, 180);
            // Draw border
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 2), hoverColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - 2, bounds.Width, 2), hoverColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, 2, bounds.Height), hoverColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - 2, bounds.Y, 2, bounds.Height), hoverColor);
        }

        // Draw selection highlight
        if (_selectedIndex >= 0 && _selectedIndex < _avatarWidgets.Count)
        {
            var selected = _avatarWidgets[_selectedIndex];
            var bounds = selected.Bounds;
            bounds.Inflate(3, 3);

            var highlightColor = new Color(200, 180, 140);
            // Draw border
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 2), highlightColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - 2, bounds.Width, 2), highlightColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, 2, bounds.Height), highlightColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - 2, bounds.Y, 2, bounds.Height), highlightColor);
        }
    }
}
