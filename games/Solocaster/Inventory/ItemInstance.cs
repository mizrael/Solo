using System;

namespace Solocaster.Inventory;

public class ItemInstance
{
    private ItemTemplate? _cachedTemplate;

    public ItemInstance(string templateId, int stackCount = 1)
    {
        UniqueId = Guid.NewGuid();
        TemplateId = templateId;
        StackCount = stackCount;
    }

    public Guid UniqueId { get; }
    public string TemplateId { get; }
    public int StackCount { get; set; }

    public ItemTemplate Template
    {
        get
        {
            _cachedTemplate ??= ItemTemplateLoader.Get(TemplateId);
            return _cachedTemplate;
        }
    }

    public bool CanStack(ItemInstance other)
    {
        return Template.Stackable &&
               TemplateId == other.TemplateId &&
               StackCount + other.StackCount <= Template.MaxStackSize;
    }

    public int AddToStack(int amount)
    {
        if (!Template.Stackable)
            return amount;

        int spaceLeft = Template.MaxStackSize - StackCount;
        int toAdd = Math.Min(amount, spaceLeft);
        StackCount += toAdd;
        return amount - toAdd;
    }

    public ItemInstance? Split(int amount)
    {
        if (amount <= 0 || amount >= StackCount)
            return null;

        StackCount -= amount;
        return new ItemInstance(TemplateId, amount);
    }

    public float TotalWeight => Template.Weight * StackCount;
}
