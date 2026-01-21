using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solocaster.Inventory;

namespace Solocaster.UI;

public enum DragSource
{
    None,
    Backpack,
    Equipment,
    Belt
}

public class DragDropManager
{
    public bool IsDragging => DraggedItem != null;

    public ItemInstance? DraggedItem { get; private set; }
    public DragSource Source { get; private set; }
    public int SourceIndex { get; private set; } = -1;
    public EquipSlot? SourceEquipSlot { get; private set; }
    public Texture2D? DraggedTexture { get; private set; }
    public Rectangle? DraggedSourceRect { get; private set; }
    public Point DragPosition { get; set; }

    public void StartDrag(ItemInstance item, DragSource source, int sourceIndex = -1, EquipSlot? equipSlot = null,
        Texture2D? texture = null, Rectangle? sourceRect = null)
    {
        DraggedItem = item;
        Source = source;
        SourceIndex = sourceIndex;
        SourceEquipSlot = equipSlot;
        DraggedTexture = texture;
        DraggedSourceRect = sourceRect;

        OnDragStarted?.Invoke(item, source);
    }

    public void EndDrag()
    {
        var item = DraggedItem;
        var source = Source;

        DraggedItem = null;
        Source = DragSource.None;
        SourceIndex = -1;
        SourceEquipSlot = null;
        DraggedTexture = null;
        DraggedSourceRect = null;

        if (item != null)
            OnDragEnded?.Invoke(item, source);
    }

    public void Render(SpriteBatch spriteBatch, int slotSize)
    {
        if (!IsDragging || DraggedTexture == null)
            return;

        var dragRect = new Rectangle(
            DragPosition.X - slotSize / 2,
            DragPosition.Y - slotSize / 2,
            slotSize - 8,
            slotSize - 8
        );

        spriteBatch.Draw(DraggedTexture, dragRect, DraggedSourceRect, Color.White * 0.8f);
    }

    public event Action<ItemInstance, DragSource>? OnDragStarted;
    public event Action<ItemInstance, DragSource>? OnDragEnded;
}
