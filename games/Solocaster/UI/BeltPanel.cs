using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo.Assets.Loaders;
using Solocaster.Components;
using Solocaster.Inventory;
using Solocaster.UI.Widgets;

namespace Solocaster.UI;

public class BeltPanel : PanelWidget
{
    private const int SlotSize = 48;
    private const int SlotPadding = 4;
    private const int PanelPadding = 8;

    private readonly InventoryComponent _inventory;
    private readonly DragDropManager _dragDropManager;
    private readonly SpriteFont _font;
    private readonly Game _game;

    private readonly List<ItemSlotWidget> _beltSlots = new();

    public BeltPanel(InventoryComponent inventory, DragDropManager dragDropManager, SpriteFont font, Game game)
    {
        _inventory = inventory;
        _dragDropManager = dragDropManager;
        _font = font;
        _game = game;

        ShowCloseButton = false;
        BackgroundColor = UITheme.Panel.BackgroundColor;
        BorderColor = UITheme.Panel.BorderColor;
        BorderWidth = UITheme.Panel.BorderWidth;
        ContentPadding = 0; // BeltPanel handles its own padding via PanelPadding

        BuildLayout();

        _inventory.OnBeltChanged += RefreshBelt;
        _dragDropManager.OnDragEnded += OnDragEnded;
    }

    private void OnDragEnded(ItemInstance item, DragSource source)
    {
        ClearDragSourceFlags();
        ClearDropTargetHighlights();
    }

    private void BuildLayout()
    {
        int beltSize = _inventory.BeltSize;
        int totalWidth = PanelPadding * 2 + beltSize * SlotSize + (beltSize - 1) * SlotPadding;
        int totalHeight = PanelPadding * 2 + SlotSize;

        Size = new Vector2(totalWidth, totalHeight);

        for (int i = 0; i < beltSize; i++)
        {
            var slot = new ItemSlotWidget
            {
                Position = new Vector2(
                    PanelPadding + i * (SlotSize + SlotPadding),
                    PanelPadding
                ),
                Size = new Vector2(SlotSize, SlotSize),
                Font = _font,
                SlotLabel = (i + 1).ToString()
            };

            int index = i;
            slot.OnDragStart += (s, item) => OnBeltSlotDragStart(index, s, item);

            _beltSlots.Add(slot);
            AddChild(slot);
        }

        RefreshBelt();
    }

    private void OnBeltSlotDragStart(int slotIndex, ItemSlotWidget slot, ItemInstance item)
    {
        _dragDropManager.StartDrag(
            item,
            DragSource.Belt,
            slotIndex,
            null,
            slot.ItemTexture,
            slot.ItemSourceRect
        );
        slot.IsDragSource = true;
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        _dragDropManager.DragPosition = new Point(mouseState.X, mouseState.Y);

        if (_dragDropManager.IsDragging)
        {
            UpdateDropTargetHighlights(mouseState);

            if (mouseState.LeftButton == ButtonState.Released)
            {
                bool handled = HandleDrop(mouseState);
                bool isOurDrag = _dragDropManager.Source == DragSource.Belt;

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

    private void UpdateDropTargetHighlights(MouseState mouseState)
    {
        ClearDropTargetHighlights();

        var mousePoint = new Point(mouseState.X, mouseState.Y);
        var draggedItem = _dragDropManager.DraggedItem;

        if (draggedItem == null)
            return;

        for (int i = 0; i < _beltSlots.Count; i++)
        {
            var slot = _beltSlots[i];
            if (slot.IsDragSource)
                continue;

            if (slot.Bounds.Contains(mousePoint))
            {
                if (_inventory.CanPutInBelt(draggedItem))
                    slot.IsValidDropTarget = true;
                else
                    slot.IsInvalidDropTarget = true;
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

        for (int i = 0; i < _beltSlots.Count; i++)
        {
            var slot = _beltSlots[i];
            if (slot.IsDragSource)
                continue;

            if (slot.Bounds.Contains(mousePoint))
            {
                HandleDropOnBeltSlot(i, draggedItem);
                return true;
            }
        }

        return false;
    }

    private void HandleDropOnBeltSlot(int targetIndex, ItemInstance draggedItem)
    {
        if (!_inventory.CanPutInBelt(draggedItem))
            return;

        var source = _dragDropManager.Source;
        var sourceIndex = _dragDropManager.SourceIndex;

        switch (source)
        {
            case DragSource.Backpack:
                _inventory.AddToBelt(draggedItem, targetIndex);
                break;

            case DragSource.Belt:
                if (sourceIndex >= 0 && sourceIndex != targetIndex)
                {
                    _inventory.SwapBeltSlots(sourceIndex, targetIndex);
                }
                break;
        }
    }

    private void ClearDropTargetHighlights()
    {
        foreach (var slot in _beltSlots)
        {
            slot.IsValidDropTarget = false;
            slot.IsInvalidDropTarget = false;
        }
    }

    private void ClearDragSourceFlags()
    {
        foreach (var slot in _beltSlots)
        {
            slot.IsDragSource = false;
        }
    }

    private void RefreshBelt()
    {
        for (int i = 0; i < _beltSlots.Count && i < _inventory.BeltSize; i++)
        {
            var slot = _beltSlots[i];
            var item = _inventory.GetBeltItem(i);

            slot.Item = item;

            if (item != null)
            {
                LoadItemTexture(slot, item);
            }
            else
            {
                slot.ItemTexture = null;
                slot.ItemSourceRect = null;
            }
        }
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

    public void PositionAtBottom(int screenWidth, int screenHeight)
    {
        Position = new Vector2(
            (screenWidth - Size.X) / 2,
            screenHeight - Size.Y - 20
        );
    }
}
