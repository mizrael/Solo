using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    private readonly SpriteFont _font;
    private readonly Game _game;
    private readonly DragDropManager _dragDropManager;

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

    public InventoryPanel(InventoryComponent inventory,DragDropManager dragDropManager, SpriteFont font, Game game)
    {
        _inventory = inventory;
        _dragDropManager = dragDropManager;
        _font = font;
        _game = game;

        ShowCloseButton = false;
        BackgroundColor = UITheme.Panel.BackgroundColor;
        BorderColor = UITheme.Panel.BorderColor;
        BorderWidth = UITheme.Panel.BorderWidth;
        ContentPadding = 0; // InventoryPanel handles its own padding

        BuildLayout();

        // Subscribe to inventory changes
        _inventory.OnBackpackChanged += RefreshBackpack;
        _inventory.OnItemEquipped += OnItemEquipped;
        _inventory.OnItemUnequipped += OnItemUnequipped;
        _dragDropManager.OnDragEnded += OnDragEnded;
    }

    private void OnDragEnded(ItemInstance item, DragSource source)
    {
        ClearDragSourceFlags();
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
            TextColor = UITheme.Text.Highlight,
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
            TextColor = UITheme.Text.Muted,
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
            TextColor = UITheme.Text.Secondary,
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
                SlotType = equipSlot,
                Item = _inventory.GetEquippedItem(equipSlot)
            };

            slot.OnItemDoubleClicked += (s, item) => OnEquipmentSlotDoubleClicked(equipSlot, item);
            slot.OnDragStart += (s, item) => OnEquipmentSlotDragStart(equipSlot, s, item);

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
            TextColor = UITheme.Text.Secondary,
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
                slot.OnItemDoubleClicked += (s, item) => OnBackpackSlotDoubleClicked(index, item);
                slot.OnDragStart += (s, item) => OnBackpackSlotDragStart(index, s, item);

                _backpackSlots.Add(slot);
                AddChild(slot);
            }
        }

        RefreshBackpack();
    }

    private void OnEquipmentSlotDoubleClicked(EquipSlot equipSlot, ItemInstance item)
    {
        // Double-click on equipped item unequips it
        _inventory.UnequipItem(equipSlot);
    }

    private void OnBackpackSlotDoubleClicked(int slotIndex, ItemInstance item)
    {
        // Double-click on backpack item equips it if possible
        if (item.Template.IsEquippable && _inventory.CanEquip(item.Template))
        {
            _inventory.EquipItem(item);
        }
    }

    private void OnEquipmentSlotDragStart(EquipSlot equipSlot, ItemSlotWidget slot, ItemInstance item)
    {
        _dragDropManager.StartDrag(
            item,
            DragSource.Equipment,
            -1,
            equipSlot,
            slot.ItemTexture,
            slot.ItemSourceRect
        );
        slot.IsDragSource = true;
    }

    private void OnBackpackSlotDragStart(int slotIndex, ItemSlotWidget slot, ItemInstance item)
    {
        _dragDropManager.StartDrag(
            item,
            DragSource.Backpack,
            slotIndex,
            null,
            slot.ItemTexture,
            slot.ItemSourceRect
        );
        slot.IsDragSource = true;
    }

    private void ClearDropTargetHighlights()
    {
        foreach (var slot in _equipmentSlots.Values)
        {
            slot.IsValidDropTarget = false;
            slot.IsInvalidDropTarget = false;
        }
        foreach (var slot in _backpackSlots)
        {
            slot.IsValidDropTarget = false;
            slot.IsInvalidDropTarget = false;
        }
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        _dragDropManager.DragPosition = new Point(mouseState.X, mouseState.Y);

        if (_dragDropManager.IsDragging)
        {
            // Update drop target highlights
            UpdateDropTargetHighlights(mouseState);

            // Check for drop (mouse released)
            if (mouseState.LeftButton == ButtonState.Released)
            {
                bool handled = HandleDrop(mouseState);
                var source = _dragDropManager.Source;
                bool isOurDrag = source == DragSource.Backpack || source == DragSource.Equipment;

                // Check if mouse is within our panel bounds
                var mousePoint = new Point(mouseState.X, mouseState.Y);
                bool mouseInPanel = Bounds.Contains(mousePoint);

                // End drag if we handled it, or if it's our drag and mouse is in our panel (cancel)
                if (handled || (isOurDrag && mouseInPanel))
                {
                    ClearDragSourceFlags();
                    _dragDropManager.EndDrag();
                }
            }
        }
    }

    private void ClearDragSourceFlags()
    {
        foreach (var slot in _equipmentSlots.Values)
        {
            slot.IsDragSource = false;
        }
        foreach (var slot in _backpackSlots)
        {
            slot.IsDragSource = false;
        }
        ClearDropTargetHighlights();
    }

    private void UpdateDropTargetHighlights(MouseState mouseState)
    {
        var draggedItem = _dragDropManager.DraggedItem;
        if (draggedItem == null)
            return;

        ClearDropTargetHighlights();

        var mousePoint = new Point(mouseState.X, mouseState.Y);

        // Check equipment slots
        foreach (var kvp in _equipmentSlots)
        {
            var slot = kvp.Value;
            if (slot.IsDragSource)
                continue;

            if (slot.Bounds.Contains(mousePoint))
            {
                if (_inventory.CanEquipToSlot(draggedItem, kvp.Key))
                    slot.IsValidDropTarget = true;
                else
                    slot.IsInvalidDropTarget = true;
                return;
            }
        }

        // Check backpack slots
        for (int i = 0; i < _backpackSlots.Count; i++)
        {
            var slot = _backpackSlots[i];
            if (slot.IsDragSource)
                continue;

            if (slot.Bounds.Contains(mousePoint))
            {
                // Backpack slots are always valid targets (for moving/swapping)
                slot.IsValidDropTarget = true;
                return;
            }
        }
    }

    private bool HandleDrop(MouseState mouseState)
    {
        var draggedItem = _dragDropManager.DraggedItem;
        if (draggedItem == null)
            return false;

        var mousePoint = new Point(mouseState.X, mouseState.Y);

        // Check equipment slots first
        foreach (var kvp in _equipmentSlots)
        {
            var slot = kvp.Value;
            if (slot.IsDragSource)
                continue;

            if (slot.Bounds.Contains(mousePoint))
            {
                HandleDropOnEquipmentSlot(kvp.Key, draggedItem);
                return true;
            }
        }

        // Check backpack slots
        for (int i = 0; i < _backpackSlots.Count; i++)
        {
            var slot = _backpackSlots[i];
            if (slot.IsDragSource)
                continue;

            if (slot.Bounds.Contains(mousePoint))
            {
                HandleDropOnBackpackSlot(i, draggedItem);
                return true;
            }
        }

        return false;
    }

    private void HandleDropOnEquipmentSlot(EquipSlot targetSlot, ItemInstance draggedItem)
    {
        // Check if item can be equipped to this slot
        if (!_inventory.CanEquipToSlot(draggedItem, targetSlot))
            return;

        var source = _dragDropManager.Source;
        var sourceEquipSlot = _dragDropManager.SourceEquipSlot;

        switch (source)
        {
            case DragSource.Equipment when sourceEquipSlot.HasValue:
            {
                // Swap equipment slots
                var targetItem = _inventory.GetEquippedItem(targetSlot);

                // Unequip source
                _inventory.UnequipItem(sourceEquipSlot.Value);

                // If target had an item, unequip it too
                if (targetItem != null)
                {
                    _inventory.UnequipItem(targetSlot);
                    // Re-equip target item to source slot if compatible
                    if (_inventory.CanEquipToSlot(targetItem, sourceEquipSlot.Value))
                    {
                        _inventory.EquipItemToSlot(targetItem, sourceEquipSlot.Value);
                    }
                }

                // Equip dragged item to target slot
                _inventory.EquipItemToSlot(draggedItem, targetSlot);
                break;
            }

            case DragSource.Backpack:
                // Dragging from backpack to equipment slot
                // If target slot is occupied, items will be swapped (existing goes to backpack)
                _inventory.EquipItemToSlot(draggedItem, targetSlot);
                break;

            case DragSource.Belt:
                // Can't equip consumables from belt
                break;
        }
    }

    private void HandleDropOnBackpackSlot(int targetIndex, ItemInstance draggedItem)
    {
        var source = _dragDropManager.Source;
        var sourceIndex = _dragDropManager.SourceIndex;
        var sourceEquipSlot = _dragDropManager.SourceEquipSlot;

        switch (source)
        {
            case DragSource.Equipment when sourceEquipSlot.HasValue:
            {
                // Dragging from equipment to backpack
                var targetItem = targetIndex < _inventory.Backpack.Count ? _inventory.Backpack[targetIndex] : null;

                // Unequip the dragged item
                _inventory.UnequipItem(sourceEquipSlot.Value);

                // If target slot had an item and it can be equipped to source slot, equip it
                if (targetItem != null && _inventory.CanEquipToSlot(targetItem, sourceEquipSlot.Value))
                {
                    _inventory.EquipItemToSlot(targetItem, sourceEquipSlot.Value);
                }
                break;
            }

            case DragSource.Backpack when sourceIndex >= 0:
            {
                // Dragging within backpack - swap or move
                var targetItem = targetIndex < _inventory.Backpack.Count ? _inventory.Backpack[targetIndex] : null;

                if (targetItem != null)
                {
                    // Swap items
                    _inventory.SwapBackpackItems(sourceIndex, targetIndex);
                }
                else
                {
                    // Move to empty slot
                    _inventory.MoveBackpackItem(sourceIndex, targetIndex);
                }
                break;
            }

            case DragSource.Belt when sourceIndex >= 0:
                // Dragging from belt to backpack - remove from belt
                _inventory.RemoveFromBelt(sourceIndex);
                break;
        }
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        base.RenderCore(spriteBatch);
        // Dragged item is rendered by UIService on top of all widgets
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
            _weightLabel.TextColor = UITheme.Text.Error;
        else if (ratio > 0.7f)
            _weightLabel.TextColor = UITheme.Text.Warning;
        else
            _weightLabel.TextColor = UITheme.Text.Muted;
    }

}
