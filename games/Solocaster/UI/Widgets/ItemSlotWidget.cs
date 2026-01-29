using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solocaster.Character;
using Solocaster.Inventory;

namespace Solocaster.UI.Widgets;

public class ItemSlotWidget : PanelWidget
{
    private const double DoubleClickTime = 300; // milliseconds
    private const int DragThreshold = 5; // pixels

    private bool _isHovered;
    private double _lastClickTime;
    private Point _mouseDownPosition;
    private bool _isMouseDown;

    public ItemSlotWidget()
    {
        ShowCloseButton = false;
        BackgroundColor = UITheme.ItemSlot.BackgroundColor;
        BorderColor = UITheme.ItemSlot.BorderColor;
        BorderWidth = UITheme.ItemSlot.BorderWidth;
        Size = new Vector2(64, 64);
    }

    public ItemInstance? Item { get; set; }
    public string SlotLabel { get; set; } = string.Empty;
    public bool ShowStackCount { get; set; } = true;
    public SpriteFont? Font { get; set; }
    public Texture2D? ItemTexture { get; set; }
    public Rectangle? ItemSourceRect { get; set; }
    public Color EmptySlotColor { get; set; } = UITheme.Selection.EmptySlot;
    public Color HoverColor { get; set; } = UITheme.Selection.SlotHover;
    public Color InvalidDropColor { get; set; } = UITheme.Selection.InvalidDrop;
    public Color ValidDropColor { get; set; } = UITheme.Selection.ValidDrop;

    // Drag-drop state (set by parent InventoryPanel)
    public bool IsValidDropTarget { get; set; }
    public bool IsInvalidDropTarget { get; set; }
    public bool IsDragSource { get; set; }
    public EquipSlot? SlotType { get; set; } // For equipment slots

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        var mousePoint = new Point(mouseState.X, mouseState.Y);
        _isHovered = Bounds.Contains(mousePoint);

        // Track mouse down for drag detection
        if (_isHovered && mouseState.LeftButton == ButtonState.Pressed &&
            previousMouseState.LeftButton == ButtonState.Released)
        {
            _mouseDownPosition = mousePoint;
            _isMouseDown = true;
        }

        // Check for drag start
        if (_isMouseDown && mouseState.LeftButton == ButtonState.Pressed && Item != null)
        {
            var distance = Math.Abs(mousePoint.X - _mouseDownPosition.X) +
                          Math.Abs(mousePoint.Y - _mouseDownPosition.Y);
            if (distance > DragThreshold)
            {
                OnDragStart?.Invoke(this, Item);
                _isMouseDown = false;
            }
        }

        // Mouse released
        if (mouseState.LeftButton == ButtonState.Released)
        {
            _isMouseDown = false;
        }

        base.UpdateCore(gameTime, mouseState, previousMouseState);
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var originalColor = BackgroundColor;

        if (IsInvalidDropTarget)
            BackgroundColor = InvalidDropColor;
        else if (IsValidDropTarget)
            BackgroundColor = ValidDropColor;
        else if (_isHovered)
            BackgroundColor = HoverColor;
        else if (Item == null)
            BackgroundColor = EmptySlotColor;

        base.RenderCore(spriteBatch);

        BackgroundColor = originalColor;

        var bounds = Bounds;

        // Draw item icon if present (but dimmed if being dragged)
        if (Item != null && ItemTexture != null && !IsDragSource)
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
                spriteBatch.DrawString(Font, countText, textPos + new Vector2(1, 1), UITheme.Text.Shadow);
                spriteBatch.DrawString(Font, countText, textPos, UITheme.Text.Primary);
            }
        }

        // Draw slot label if empty and has label
        if (Item == null && !string.IsNullOrEmpty(SlotLabel) && Font != null && !IsValidDropTarget && !IsInvalidDropTarget)
        {
            var labelSize = Font.MeasureString(SlotLabel);
            var labelPos = ScreenPosition + (Size - labelSize) / 2;
            spriteBatch.DrawString(Font, SlotLabel, labelPos, UITheme.Text.Placeholder);
        }
    }

    protected override void OnMouseClick(Point mousePosition)
    {
        var currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
        var timeSinceLastClick = currentTime - _lastClickTime;

        if (timeSinceLastClick <= DoubleClickTime && Item != null)
        {
            // Double click detected
            OnItemDoubleClicked?.Invoke(this, Item);
            _lastClickTime = 0; // Reset to prevent triple-click
        }
        else
        {
            _lastClickTime = currentTime;
            OnItemClicked?.Invoke(this, Item);
        }

        base.OnMouseClick(mousePosition);
    }

    public event Action<ItemSlotWidget, ItemInstance?>? OnItemClicked;
    public event Action<ItemSlotWidget, ItemInstance>? OnItemDoubleClicked;
    public event Action<ItemSlotWidget, ItemInstance>? OnDragStart;

    public override string? GetTooltipText()
    {
        if (!_isHovered || Item == null)
            return null;

        var template = Item.Template;
        var sb = new StringBuilder();

        // Item name
        sb.Append(template.Name);

        // Stat modifiers
        if (template.StatModifiers.Count > 0)
        {
            sb.AppendLine();
            foreach (var mod in template.StatModifiers)
            {
                var sign = mod.Value >= 0 ? "+" : "";
                sb.AppendLine($"{sign}{mod.Value:0} {FormatStatName(mod.Key)}");
            }
        }

        // Requirements
        if (template.Requirements.Count > 0)
        {
            sb.AppendLine();
            sb.Append("Requires: ");
            var first = true;
            foreach (var req in template.Requirements)
            {
                if (!first) sb.Append(", ");
                sb.Append($"{GetStatAbbreviation(req.Key)} {req.Value:0}");
                first = false;
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatStatName(Stats stat)
    {
        return stat switch
        {
            Stats.Strength => "Strength",
            Stats.Agility => "Agility",
            Stats.Vitality => "Vitality",
            Stats.Intelligence => "Intelligence",
            Stats.MaxHealth => "Max Health",
            Stats.MaxWeight => "Max Weight",
            Stats.Damage => "Damage",
            Stats.Defense => "Defense",
            Stats.AttackSpeed => "Attack Speed",
            Stats.CriticalChance => "Critical Chance",
            Stats.MaxMana => "Max Mana",
            Stats.ManaRegen => "Mana Regen",
            Stats.SpellPower => "Spell Power",
            _ => stat.ToString()
        };
    }

    private static string GetStatAbbreviation(Stats stat)
    {
        return stat switch
        {
            Stats.Strength => "STR",
            Stats.Agility => "AGI",
            Stats.Vitality => "VIT",
            Stats.Intelligence => "INT",
            Stats.MaxHealth => "HP",
            Stats.MaxWeight => "WT",
            Stats.Damage => "DMG",
            Stats.Defense => "DEF",
            Stats.AttackSpeed => "SPD",
            Stats.CriticalChance => "CRIT",
            Stats.MaxMana => "MP",
            Stats.ManaRegen => "MPR",
            Stats.SpellPower => "PWR",
            _ => stat.ToString()
        };
    }
}
