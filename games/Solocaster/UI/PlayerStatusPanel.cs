using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets.Loaders;
using Solocaster.Components;
using Solocaster.Inventory;
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
        int totalHeight = Padding * 2 + AvatarSize;
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
        int healthBarY = (int)screenPos.Y + Padding + (AvatarSize - BarHeight * 2 - BarSpacing) / 2;
        int manaBarY = healthBarY + BarHeight + BarSpacing;

        // Health bar
        float healthRatio = _stats.CurrentHealth / _stats.GetTotalStat(StatType.MaxHealth);
        DrawBar(spriteBatch, barX, healthBarY, healthRatio, new Color(180, 40, 40), new Color(60, 20, 20));

        // Mana bar
        float manaRatio = _stats.CurrentMana / _stats.GetTotalStat(StatType.MaxMana);
        DrawBar(spriteBatch, barX, manaBarY, manaRatio, new Color(40, 80, 180), new Color(20, 30, 60));
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
        var borderColor = new Color(80, 80, 80);
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
