using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solocaster.Inventory;

namespace Solocaster.Components;

public class PickupableComponent : Component
{
    public PickupableComponent(GameObject owner) : base(owner)
    {
    }

    public required string ItemTemplateId { get; init; }
    public int Quantity { get; init; } = 1;
    public float PickupRadius { get; init; } = 1f;
    public SpatialGrid? SpatialGrid { get; init; }

    public ItemInstance CreateItemInstance()
    {
        return new ItemInstance(ItemTemplateId, Quantity);
    }

    public bool IsInRange(Vector2 position)
    {
        var transform = Owner.Components.Get<TransformComponent>();
        if (transform == null)
            return false;

        float distance = Vector2.Distance(transform.World.Position, position);
        return distance <= PickupRadius;
    }

    public void OnPickedUp()
    {
        SpatialGrid?.Remove(Owner);
        Owner.Enabled = false;
        Owner.Parent?.RemoveChild(Owner);
    }
}
