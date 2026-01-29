using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets.Loaders;
using Solocaster.Character;
using Solocaster.Components;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI;

public class PlayerStatusPanel : PanelWidget
{
    private const int AvatarSize = 48;
    private const int BarWidth = 100;
    private const int BarHeight = 12;
    private const int Padding = 8;
    private const int BarSpacing = 4;

    private readonly StatsComponent _stats;
    private readonly Game _game;

    private Texture2D? _avatarTexture;
    private Rectangle? _avatarSourceRect;
    private Texture2D? _pixelTexture;

    public PlayerStatusPanel(StatsComponent stats, Game game)
    {
        _stats = stats;
        _game = game;

        ShowCloseButton = false;
        BackgroundColor = UITheme.Panel.BackgroundColor;
        BorderColor = UITheme.Panel.BorderColor;
        BorderWidth = UITheme.Panel.BorderWidth;
        ContentPadding = 0; // PlayerStatusPanel handles its own padding

        int totalWidth = Padding * 3 + AvatarSize + BarWidth;
        int totalHeight = Padding * 2 + Math.Max(AvatarSize, BarHeight * 3 + BarSpacing * 2);
        Size = new Vector2(totalWidth, totalHeight);

        LoadAvatar();
        CreatePixelTexture();

        _stats.OnStatsChanged += OnStatsChanged;
    }

    private void LoadAvatar()
    {
        try
        {
            var spriteSheet = SpriteSheetLoader.Get("avatars", _game);
            var avatarName = GameState.CurrentCharacter?.AvatarSpriteName ?? "human_warrior_male";
            var sprite = spriteSheet.Get(avatarName);
            _avatarTexture = sprite.Texture;
            _avatarSourceRect = sprite.Bounds;
        }
        catch
        {
            // Avatar not found, will skip rendering
        }
    }

    private void CreatePixelTexture()
    {
        _pixelTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    private void OnStatsChanged()
    {
        // Stats changed, will be reflected in next render
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        base.RenderCore(spriteBatch);

        if (_pixelTexture == null)
            return;

        var screenPos = ScreenPosition;

        // Draw avatar
        if (_avatarTexture != null)
        {
            var avatarRect = new Rectangle(
                (int)screenPos.X + Padding,
                (int)screenPos.Y + Padding,
                AvatarSize,
                AvatarSize
            );
            spriteBatch.Draw(_avatarTexture, avatarRect, _avatarSourceRect, Color.White);
        }

        // Calculate bar positions
        int barX = (int)screenPos.X + Padding * 2 + AvatarSize;
        int healthBarY = (int)screenPos.Y + Padding + (AvatarSize - BarHeight * 3 - BarSpacing * 2) / 2;
        int manaBarY = healthBarY + BarHeight + BarSpacing;
        int staminaBarY = manaBarY + BarHeight + BarSpacing;

        // Health bar
        float healthRatio = _stats.CurrentHealth / _stats.GetTotalStat(Stats.MaxHealth);
        DrawBar(spriteBatch, barX, healthBarY, healthRatio, UITheme.StatusBar.HealthFill, UITheme.StatusBar.HealthBackground);

        // Mana bar
        float manaRatio = _stats.CurrentMana / _stats.GetTotalStat(Stats.MaxMana);
        DrawBar(spriteBatch, barX, manaBarY, manaRatio, UITheme.StatusBar.ManaFill, UITheme.StatusBar.ManaBackground);

        // Stamina bar
        float staminaRatio = _stats.CurrentStamina / _stats.MaxStamina;
        Color staminaFill = _stats.IsExhausted
            ? PulseColor(UITheme.StatusBar.StaminaFill, 0.5f)
            : UITheme.StatusBar.StaminaFill;
        DrawBar(spriteBatch, barX, staminaBarY, staminaRatio, staminaFill, UITheme.StatusBar.StaminaBackground);
    }

    private static Color PulseColor(Color baseColor, float intensity)
    {
        float pulse = (float)(Math.Sin(DateTime.Now.Ticks / 1000000.0 * 10) * 0.5 + 0.5);
        float factor = 1f - (pulse * intensity);
        return new Color(
            (int)(baseColor.R * factor),
            (int)(baseColor.G * factor),
            (int)(baseColor.B * factor),
            baseColor.A
        );
    }

    private void DrawBar(SpriteBatch spriteBatch, int x, int y, float ratio, Color fillColor, Color bgColor)
    {
        if (_pixelTexture == null)
            return;

        // Background
        var bgRect = new Rectangle(x, y, BarWidth, BarHeight);
        spriteBatch.Draw(_pixelTexture, bgRect, bgColor);

        // Fill
        int fillWidth = (int)(BarWidth * Math.Clamp(ratio, 0, 1));
        if (fillWidth > 0)
        {
            var fillRect = new Rectangle(x, y, fillWidth, BarHeight);
            spriteBatch.Draw(_pixelTexture, fillRect, fillColor);
        }

        // Border
        var borderColor = UITheme.Panel.BorderColor;
        spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, BarWidth, 1), borderColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + BarHeight - 1, BarWidth, 1), borderColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, 1, BarHeight), borderColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(x + BarWidth - 1, y, 1, BarHeight), borderColor);
    }

    public void PositionTopRight(int screenWidth)
    {
        Position = new Vector2(screenWidth - Size.X - 10, 10);
    }
}
