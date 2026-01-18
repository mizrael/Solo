using Solo;
using Solo.Components;
using Solocaster.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solocaster.Components;

public enum AddItemResult
{
    Success,
    TooHeavy,
    InvalidItem
}

public enum EquipResult
{
    Success,
    RequirementsNotMet,
    SlotOccupied,
    NotEquippable,
    NotInBackpack
}

public class InventoryComponent : Component
{
    private readonly Dictionary<EquipSlot, ItemInstance?> _equipment = new()
    {
        [EquipSlot.Head] = null,
        [EquipSlot.Neck] = null,
        [EquipSlot.Chest] = null,
        [EquipSlot.LeftHand] = null,
        [EquipSlot.RightHand] = null,
        [EquipSlot.Legs] = null,
        [EquipSlot.LeftRing] = null,
        [EquipSlot.RightRing] = null
    };

    private readonly List<ItemInstance> _backpack = new();
    private StatsComponent? _stats;

    public InventoryComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _stats = Owner.Components.Get<StatsComponent>();
        base.InitCore();
    }

    public IReadOnlyDictionary<EquipSlot, ItemInstance?> Equipment => _equipment;
    public IReadOnlyList<ItemInstance> Backpack => _backpack;

    public float CurrentWeight => _backpack.Sum(i => i.TotalWeight) +
                                   _equipment.Values.Where(i => i != null).Sum(i => i!.TotalWeight);

    public float MaxWeight => _stats?.GetTotalStat(StatType.MaxWeight) ?? 100f;

    public AddItemResult AddItem(ItemInstance item)
    {
        if (item == null)
            return AddItemResult.InvalidItem;

        float newWeight = CurrentWeight + item.TotalWeight;
        if (newWeight > MaxWeight)
            return AddItemResult.TooHeavy;

        // Try to stack with existing items
        if (item.Template.Stackable)
        {
            foreach (var existing in _backpack)
            {
                if (existing.TemplateId == item.TemplateId && existing.StackCount < existing.Template.MaxStackSize)
                {
                    int remaining = existing.AddToStack(item.StackCount);
                    if (remaining <= 0)
                    {
                        OnBackpackChanged?.Invoke();
                        return AddItemResult.Success;
                    }
                    item.StackCount = remaining;
                }
            }
        }

        _backpack.Add(item);
        OnBackpackChanged?.Invoke();
        return AddItemResult.Success;
    }

    public bool RemoveItem(ItemInstance item)
    {
        if (_backpack.Remove(item))
        {
            OnBackpackChanged?.Invoke();
            return true;
        }
        return false;
    }

    public bool RemoveItemByTemplateId(string templateId, int count = 1)
    {
        int remaining = count;
        for (int i = _backpack.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var item = _backpack[i];
            if (item.TemplateId != templateId)
                continue;

            if (item.StackCount <= remaining)
            {
                remaining -= item.StackCount;
                _backpack.RemoveAt(i);
            }
            else
            {
                item.StackCount -= remaining;
                remaining = 0;
            }
        }

        if (remaining < count)
        {
            OnBackpackChanged?.Invoke();
            return true;
        }
        return false;
    }

    public EquipResult EquipItem(ItemInstance item, EquipSlot? targetSlot = null)
    {
        if (!item.Template.IsEquippable)
            return EquipResult.NotEquippable;

        if (!_backpack.Contains(item))
            return EquipResult.NotInBackpack;

        if (_stats != null && !_stats.MeetsRequirements(item.Template))
            return EquipResult.RequirementsNotMet;

        // Determine target slot
        EquipSlot slot = targetSlot ?? GetAvailableSlot(item.Template.EquipSlot);

        // Unequip existing item if any
        var existingItem = _equipment[slot];
        if (existingItem != null)
        {
            UnequipItemInternal(slot);
        }

        // Equip the new item
        _backpack.Remove(item);
        _equipment[slot] = item;
        _stats?.OnItemEquipped(item);

        OnItemEquipped?.Invoke(item, slot);
        OnBackpackChanged?.Invoke();

        return EquipResult.Success;
    }

    public bool UnequipItem(EquipSlot slot)
    {
        if (_equipment[slot] == null)
            return false;

        var result = UnequipItemInternal(slot);
        if (result)
        {
            OnBackpackChanged?.Invoke();
        }
        return result;
    }

    private bool UnequipItemInternal(EquipSlot slot)
    {
        var item = _equipment[slot];
        if (item == null)
            return false;

        _equipment[slot] = null;
        _backpack.Add(item);
        _stats?.OnItemUnequipped(item);

        OnItemUnequipped?.Invoke(item, slot);
        return true;
    }

    private EquipSlot GetAvailableSlot(EquipSlot itemSlot)
    {
        // For rings, find an available slot (prefer left, then right)
        if (itemSlot == EquipSlot.LeftRing || itemSlot == EquipSlot.RightRing)
        {
            if (_equipment[EquipSlot.LeftRing] == null)
                return EquipSlot.LeftRing;
            return EquipSlot.RightRing;
        }

        return itemSlot;
    }

    public ItemInstance? GetEquippedItem(EquipSlot slot)
    {
        return _equipment.TryGetValue(slot, out var item) ? item : null;
    }

    public bool CanCarry(float additionalWeight)
    {
        return CurrentWeight + additionalWeight <= MaxWeight;
    }

    public bool CanEquip(ItemTemplate template)
    {
        if (!template.IsEquippable)
            return false;
        if (_stats != null && !_stats.MeetsRequirements(template))
            return false;
        return true;
    }

    public bool CanEquipToSlot(ItemInstance item, EquipSlot targetSlot)
    {
        if (!item.Template.IsEquippable)
            return false;

        var itemSlot = item.Template.EquipSlot;

        // Ring can go to either ring slot
        if (itemSlot == EquipSlot.LeftRing || itemSlot == EquipSlot.RightRing)
            return targetSlot == EquipSlot.LeftRing || targetSlot == EquipSlot.RightRing;

        return itemSlot == targetSlot;
    }

    public void SwapBackpackItems(int index1, int index2)
    {
        if (index1 < 0 || index1 >= _backpack.Count ||
            index2 < 0 || index2 >= _backpack.Count ||
            index1 == index2)
            return;

        (_backpack[index1], _backpack[index2]) = (_backpack[index2], _backpack[index1]);
        OnBackpackChanged?.Invoke();
    }

    public void MoveBackpackItem(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _backpack.Count || fromIndex == toIndex)
            return;

        var item = _backpack[fromIndex];
        _backpack.RemoveAt(fromIndex);

        // Adjust target index if needed
        if (toIndex > fromIndex)
            toIndex--;

        if (toIndex >= _backpack.Count)
            _backpack.Add(item);
        else
            _backpack.Insert(Math.Max(0, toIndex), item);

        OnBackpackChanged?.Invoke();
    }

    public EquipResult EquipItemToSlot(ItemInstance item, EquipSlot targetSlot)
    {
        if (!item.Template.IsEquippable)
            return EquipResult.NotEquippable;

        if (!_backpack.Contains(item))
            return EquipResult.NotInBackpack;

        if (!CanEquipToSlot(item, targetSlot))
            return EquipResult.NotEquippable;

        if (_stats != null && !_stats.MeetsRequirements(item.Template))
            return EquipResult.RequirementsNotMet;

        // Unequip existing item if any
        var existingItem = _equipment[targetSlot];
        if (existingItem != null)
        {
            UnequipItemInternal(targetSlot);
        }

        // Equip the new item
        _backpack.Remove(item);
        _equipment[targetSlot] = item;
        _stats?.OnItemEquipped(item);

        OnItemEquipped?.Invoke(item, targetSlot);
        OnBackpackChanged?.Invoke();

        return EquipResult.Success;
    }

    public int GetBackpackIndex(ItemInstance item)
    {
        return _backpack.IndexOf(item);
    }

    public event Action<ItemInstance, EquipSlot>? OnItemEquipped;
    public event Action<ItemInstance, EquipSlot>? OnItemUnequipped;
    public event Action? OnBackpackChanged;
}
