using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solocaster.Inventory;

namespace Solocaster.UI.Widgets;

public class ItemSlotWidget : PanelWidget
{
    private bool _isHovered;

    public ItemSlotWidget()
    {
        BackgroundColor = new Color(30, 30, 30, 200);
        BorderColor = new Color(70, 70, 70);
        Size = new Vector2(64, 64);
    }

    public ItemInstance? Item { get; set; }
    public string SlotLabel { get; set; } = string.Empty;
    public bool ShowStackCount { get; set; } = true;
    public SpriteFont? Font { get; set; }
    public Texture2D? ItemTexture { get; set; }
    public Rectangle? ItemSourceRect { get; set; }
    public Color EmptySlotColor { get; set; } = new Color(50, 50, 50, 150);
    public Color HoverColor { get; set; } = new Color(60, 60, 80, 220);

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        var mousePoint = new Point(mouseState.X, mouseState.Y);
        _isHovered = Bounds.Contains(mousePoint);

        base.UpdateCore(gameTime, mouseState, previousMouseState);
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var originalColor = BackgroundColor;
        if (_isHovered)
            BackgroundColor = HoverColor;
        else if (Item == null)
            BackgroundColor = EmptySlotColor;

        base.RenderCore(spriteBatch);

        BackgroundColor = originalColor;

        var bounds = Bounds;

        // Draw item icon if present
        if (Item != null && ItemTexture != null)
        {
            var iconPadding = 4;
            var iconRect = new Rectangle(
                bounds.X + iconPadding,
                bounds.Y + iconPadding,
                bounds.Width - iconPadding * 2,
                bounds.Height - iconPadding * 2
            );

            spriteBatch.Draw(ItemTexture, iconRect, ItemSourceRect, Color.White);

            // Draw stack count
            if (ShowStackCount && Item.StackCount > 1 && Font != null)
            {
                var countText = Item.StackCount.ToString();
                var textSize = Font.MeasureString(countText);
                var textPos = new Vector2(
                    bounds.Right - textSize.X - 4,
                    bounds.Bottom - textSize.Y - 2
                );

                // Draw shadow
                spriteBatch.DrawString(Font, countText, textPos + new Vector2(1, 1), Color.Black);
                spriteBatch.DrawString(Font, countText, textPos, Color.White);
            }
        }

        // Draw slot label if empty and has label
        if (Item == null && !string.IsNullOrEmpty(SlotLabel) && Font != null)
        {
            var labelSize = Font.MeasureString(SlotLabel);
            var labelPos = ScreenPosition + (Size - labelSize) / 2;
            spriteBatch.DrawString(Font, SlotLabel, labelPos, new Color(100, 100, 100));
        }
    }

    protected override void OnMouseClick(Point mousePosition)
    {
        OnItemClicked?.Invoke(this, Item);
        base.OnMouseClick(mousePosition);
    }

    public event Action<ItemSlotWidget, ItemInstance?>? OnItemClicked;

    public override string? GetTooltipText()
    {
        if (_isHovered && Item != null)
            return Item.Template.Name;
        return null;
    }
}
