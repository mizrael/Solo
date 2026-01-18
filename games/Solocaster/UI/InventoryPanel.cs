using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets.Loaders;
using Solocaster.Components;
using Solocaster.Inventory;
using Solocaster.UI.Widgets;

namespace Solocaster.UI;

public class InventoryPanel : PanelWidget
{
    private const int SlotSize = 64;
    private const int SlotPadding = 8;
    private const int BackpackColumns = 5;
    private const int BackpackRows = 4;

    private readonly InventoryComponent _inventory;
    private readonly StatsComponent _stats;
    private readonly SpriteFont _font;
    private readonly Game _game;

    private readonly Dictionary<EquipSlot, ItemSlotWidget> _equipmentSlots = new();
    private readonly List<ItemSlotWidget> _backpackSlots = new();
    private LabelWidget? _weightLabel;
    private LabelWidget? _titleLabel;

    // Equipment slot layout positions (relative to equipment section)
    private static readonly Dictionary<EquipSlot, Vector2> EquipmentSlotPositions = new()
    {
        [EquipSlot.Head] = new Vector2(1, 0),
        [EquipSlot.Neck] = new Vector2(2, 0),
        [EquipSlot.Chest] = new Vector2(1, 1),
        [EquipSlot.LeftHand] = new Vector2(0, 1),
        [EquipSlot.RightHand] = new Vector2(2, 1),
        [EquipSlot.Legs] = new Vector2(1, 2),
        [EquipSlot.LeftRing] = new Vector2(0, 2),
        [EquipSlot.RightRing] = new Vector2(2, 2)
    };

    private static readonly Dictionary<EquipSlot, string> SlotLabels = new()
    {
        [EquipSlot.Head] = "Head",
        [EquipSlot.Neck] = "Neck",
        [EquipSlot.Chest] = "Body",
        [EquipSlot.LeftHand] = "L",
        [EquipSlot.RightHand] = "R",
        [EquipSlot.Legs] = "Legs",
        [EquipSlot.LeftRing] = "LR",
        [EquipSlot.RightRing] = "RR"
    };

    public InventoryPanel(InventoryComponent inventory, StatsComponent stats, SpriteFont font, Game game)
    {
        _inventory = inventory;
        _stats = stats;
        _font = font;
        _game = game;

        BackgroundColor = new Color(20, 20, 25, 240);
        BorderColor = new Color(100, 80, 60);
        BorderWidth = 3;

        BuildLayout();

        // Subscribe to inventory changes
        _inventory.OnBackpackChanged += RefreshBackpack;
        _inventory.OnItemEquipped += OnItemEquipped;
        _inventory.OnItemUnequipped += OnItemUnequipped;
    }

    private void BuildLayout()
    {
        int padding = 16;
        int equipmentWidth = 3 * (SlotSize + SlotPadding) + SlotPadding;
        int backpackWidth = BackpackColumns * (SlotSize + SlotPadding) + SlotPadding;
        int totalWidth = padding * 2 + equipmentWidth + 20 + backpackWidth;
        int totalHeight = padding * 2 + 40 + BackpackRows * (SlotSize + SlotPadding) + SlotPadding + 30;

        Size = new Vector2(totalWidth, totalHeight);

        // Title
        _titleLabel = new LabelWidget
        {
            Text = "Inventory",
            Font = _font,
            TextColor = new Color(200, 180, 140),
            Position = new Vector2(padding, padding),
            Size = new Vector2(totalWidth - padding * 2, 30),
            CenterHorizontally = true
        };
        AddChild(_titleLabel);

        int contentY = padding + 40;

        // Equipment section
        BuildEquipmentSection(padding, contentY, equipmentWidth);

        // Backpack section
        int backpackX = padding + equipmentWidth + 20;
        BuildBackpackSection(backpackX, contentY, backpackWidth);

        // Weight label
        _weightLabel = new LabelWidget
        {
            Font = _font,
            TextColor = Color.Gray,
            Position = new Vector2(padding, totalHeight - padding - 20),
            Size = new Vector2(totalWidth - padding * 2, 20)
        };
        AddChild(_weightLabel);

        UpdateWeightLabel();
    }

    private void BuildEquipmentSection(int startX, int startY, int width)
    {
        // Section label
        var label = new LabelWidget
        {
            Text = "Equipment",
            Font = _font,
            TextColor = Color.LightGray,
            Position = new Vector2(startX, startY),
            Size = new Vector2(width, 20)
        };
        AddChild(label);

        int slotStartY = startY + 25;

        foreach (var kvp in EquipmentSlotPositions)
        {
            var equipSlot = kvp.Key;
            var gridPos = kvp.Value;

            var slot = new ItemSlotWidget
            {
                Position = new Vector2(
                    startX + gridPos.X * (SlotSize + SlotPadding),
                    slotStartY + gridPos.Y * (SlotSize + SlotPadding)
                ),
                Size = new Vector2(SlotSize, SlotSize),
                Font = _font,
                SlotLabel = SlotLabels[equipSlot],
                Item = _inventory.GetEquippedItem(equipSlot)
            };

            slot.OnItemClicked += (s, item) => OnEquipmentSlotClicked(equipSlot, item);

            _equipmentSlots[equipSlot] = slot;
            AddChild(slot);
        }

        RefreshEquipmentSlots();
    }

    private void BuildBackpackSection(int startX, int startY, int width)
    {
        // Section label
        var label = new LabelWidget
        {
            Text = "Backpack",
            Font = _font,
            TextColor = Color.LightGray,
            Position = new Vector2(startX, startY),
            Size = new Vector2(width, 20)
        };
        AddChild(label);

        int slotStartY = startY + 25;

        for (int row = 0; row < BackpackRows; row++)
        {
            for (int col = 0; col < BackpackColumns; col++)
            {
                var slot = new ItemSlotWidget
                {
                    Position = new Vector2(
                        startX + col * (SlotSize + SlotPadding),
                        slotStartY + row * (SlotSize + SlotPadding)
                    ),
                    Size = new Vector2(SlotSize, SlotSize),
                    Font = _font
                };

                int index = row * BackpackColumns + col;
                slot.OnItemClicked += (s, item) => OnBackpackSlotClicked(index, item);

                _backpackSlots.Add(slot);
                AddChild(slot);
            }
        }

        RefreshBackpack();
    }

    private void OnEquipmentSlotClicked(EquipSlot equipSlot, ItemInstance? item)
    {
        if (item != null)
        {
            _inventory.UnequipItem(equipSlot);
        }
    }

    private void OnBackpackSlotClicked(int slotIndex, ItemInstance? item)
    {
        if (item == null)
            return;

        if (item.Template.IsEquippable && _inventory.CanEquip(item.Template))
        {
            _inventory.EquipItem(item);
        }
    }

    private void OnItemEquipped(ItemInstance item, EquipSlot equipSlot)
    {
        RefreshEquipmentSlots();
        UpdateWeightLabel();
    }

    private void OnItemUnequipped(ItemInstance item, EquipSlot equipSlot)
    {
        RefreshEquipmentSlots();
        UpdateWeightLabel();
    }

    private void RefreshEquipmentSlots()
    {
        foreach (var kvp in _equipmentSlots)
        {
            var item = _inventory.GetEquippedItem(kvp.Key);
            kvp.Value.Item = item;

            if (item != null)
            {
                LoadItemTexture(kvp.Value, item);
            }
            else
            {
                kvp.Value.ItemTexture = null;
                kvp.Value.ItemSourceRect = null;
            }
        }
    }

    private void RefreshBackpack()
    {
        var backpackItems = _inventory.Backpack;

        for (int i = 0; i < _backpackSlots.Count; i++)
        {
            var slot = _backpackSlots[i];

            if (i < backpackItems.Count)
            {
                var item = backpackItems[i];
                slot.Item = item;
                LoadItemTexture(slot, item);
            }
            else
            {
                slot.Item = null;
                slot.ItemTexture = null;
                slot.ItemSourceRect = null;
            }
        }

        UpdateWeightLabel();
    }

    private void LoadItemTexture(ItemSlotWidget slot, ItemInstance item)
    {
        var iconPath = item.Template.IconPath;
        if (string.IsNullOrEmpty(iconPath))
        {
            iconPath = item.Template.WorldSpritePath;
        }

        if (string.IsNullOrEmpty(iconPath))
        {
            slot.ItemTexture = null;
            slot.ItemSourceRect = null;
            return;
        }

        // Parse sprite reference (format: "sheetname:spritename")
        var parts = iconPath.Split(':');
        if (parts.Length != 2)
        {
            slot.ItemTexture = null;
            slot.ItemSourceRect = null;
            return;
        }

        try
        {
            var sheetName = parts[0];
            var spriteName = parts[1];
            var spriteSheet = SpriteSheetLoader.Get(sheetName, _game);
            var sprite = spriteSheet.Get(spriteName);

            slot.ItemTexture = sprite.Texture;
            slot.ItemSourceRect = sprite.Bounds;
        }
        catch
        {
            slot.ItemTexture = null;
            slot.ItemSourceRect = null;
        }
    }

    private void UpdateWeightLabel()
    {
        if (_weightLabel == null)
            return;

        float current = _inventory.CurrentWeight;
        float max = _inventory.MaxWeight;
        _weightLabel.Text = $"Weight: {current:F1} / {max:F1}";

        float ratio = current / max;
        if (ratio > 0.9f)
            _weightLabel.TextColor = Color.Red;
        else if (ratio > 0.7f)
            _weightLabel.TextColor = Color.Yellow;
        else
            _weightLabel.TextColor = Color.Gray;
    }

    public void Toggle()
    {
        Visible = !Visible;
    }

    public void CenterOnScreen(int screenWidth, int screenHeight)
    {
        Position = new Vector2(
            (screenWidth - Size.X) / 2,
            (screenHeight - Size.Y) / 2
        );
    }
}
