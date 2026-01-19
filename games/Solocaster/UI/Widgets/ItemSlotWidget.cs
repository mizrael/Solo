using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    public Color InvalidDropColor { get; set; } = new Color(200, 50, 50, 180);
    public Color ValidDropColor { get; set; } = new Color(50, 200, 50, 180);

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
                spriteBatch.DrawString(Font, countText, textPos + new Vector2(1, 1), Color.Black);
                spriteBatch.DrawString(Font, countText, textPos, Color.White);
            }
        }

        // Draw slot label if empty and has label
        if (Item == null && !string.IsNullOrEmpty(SlotLabel) && Font != null && !IsValidDropTarget && !IsInvalidDropTarget)
        {
            var labelSize = Font.MeasureString(SlotLabel);
            var labelPos = ScreenPosition + (Size - labelSize) / 2;
            spriteBatch.DrawString(Font, SlotLabel, labelPos, new Color(100, 100, 100));
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

    private static string FormatStatName(StatType stat)
    {
        return stat switch
        {
            StatType.Strength => "Strength",
            StatType.Agility => "Agility",
            StatType.Vitality => "Vitality",
            StatType.Intelligence => "Intelligence",
            StatType.MaxHealth => "Max Health",
            StatType.MaxWeight => "Max Weight",
            StatType.Damage => "Damage",
            StatType.Defense => "Defense",
            StatType.AttackSpeed => "Attack Speed",
            StatType.CriticalChance => "Critical Chance",
            StatType.MaxMana => "Max Mana",
            StatType.ManaRegen => "Mana Regen",
            StatType.SpellPower => "Spell Power",
            _ => stat.ToString()
        };
    }

    private static string GetStatAbbreviation(StatType stat)
    {
        return stat switch
        {
            StatType.Strength => "STR",
            StatType.Agility => "AGI",
            StatType.Vitality => "VIT",
            StatType.Intelligence => "INT",
            StatType.MaxHealth => "HP",
            StatType.MaxWeight => "WT",
            StatType.Damage => "DMG",
            StatType.Defense => "DEF",
            StatType.AttackSpeed => "SPD",
            StatType.CriticalChance => "CRIT",
            StatType.MaxMana => "MP",
            StatType.ManaRegen => "MPR",
            StatType.SpellPower => "PWR",
            _ => stat.ToString()
        };
    }
}
