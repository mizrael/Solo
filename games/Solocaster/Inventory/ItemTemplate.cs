using System.Collections.Generic;

namespace Solocaster.Inventory;

public class ItemTemplate
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string Description { get; init; } = string.Empty;
    public string IconPath { get; init; } = string.Empty;
    public string WorldSpritePath { get; init; } = string.Empty;
    public float WorldSpriteScale { get; init; } = 1f;
    public ItemType ItemType { get; init; } = ItemType.Misc;
    public EquipSlot EquipSlot { get; init; } = EquipSlot.None;
    public float Weight { get; init; } = 1f;
    public bool Stackable { get; init; } = false;
    public int MaxStackSize { get; init; } = 1;
    public Dictionary<StatType, float> StatModifiers { get; init; } = new();
    public Dictionary<StatType, float> Requirements { get; init; } = new();

    public bool IsEquippable => EquipSlot != EquipSlot.None;
}
