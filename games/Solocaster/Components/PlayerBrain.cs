using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Components;
using Solocaster.Entities;
using Solocaster.Inventory;
using Solocaster.UI;
using System;

namespace Solocaster.Components;

public class PlayerBrain : Component
{
    private Vector2 _plane = new(0, .45f);

    private TransformComponent _transform;
    private InventoryComponent? _inventory;

    private readonly Map _map;
    private KeyboardState _previousKeyboardState;

    public InventoryPanel? InventoryPanel { get; set; }
    public GameObject? EntityContainer { get; set; }

    public PlayerBrain(GameObject owner, Map map) : base(owner)
    {
        _map = map;
    }

    protected override void InitCore()
    {
        _transform = this.Owner.Components.Get<TransformComponent>();
        _inventory = this.Owner.Components.Get<InventoryComponent>();

        base.InitCore();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        float ms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        float moveSpeed = ms * .015f;
        float rotSpeed = ms * .005f;

        var keyboardState = Keyboard.GetState();

        // Toggle inventory with Tab
        if (keyboardState.IsKeyDown(Keys.Tab) && !_previousKeyboardState.IsKeyDown(Keys.Tab))
        {
            InventoryPanel?.Toggle();
        }

        // Try to pick up nearby items with E (separate from door opening)
        if (keyboardState.IsKeyDown(Keys.E) && !_previousKeyboardState.IsKeyDown(Keys.E))
        {
            if (!TryPickupNearbyItem())
            {
                TryOpenDoor();
            }
            _previousKeyboardState = keyboardState;
            return;
        }

        _previousKeyboardState = keyboardState;

        float moveAmount = 0;
        if (keyboardState.IsKeyDown(Keys.W))
            moveAmount = moveSpeed;
        else if (keyboardState.IsKeyDown(Keys.S))
            moveAmount = -moveSpeed;

        if (moveAmount != 0)
        {
            var moveStep = _transform.World.Direction * moveAmount;
            if (!_map.IsBlocked((int)(_transform.World.Position.X + moveStep.X), (int)_transform.World.Position.Y))
                _transform.Local.Position.X += moveStep.X;

            if (!_map.IsBlocked((int)_transform.World.Position.X, (int)(_transform.World.Position.Y + moveStep.Y)))
                _transform.Local.Position.Y += moveStep.Y;
        }

        if (keyboardState.IsKeyDown(Keys.A))
        {
            Vector2 oldDirection = _transform.Local.Direction;
            var cos = MathF.Cos(-rotSpeed);
            var sin = MathF.Sin(-rotSpeed);

            _transform.Local.Direction = new(
                oldDirection.X * cos - oldDirection.Y * sin,
                oldDirection.X * sin + oldDirection.Y * cos);

            Vector2 oldPlane = _plane;
            _plane.X = _plane.X * cos - _plane.Y * sin;
            _plane.Y = oldPlane.X * sin + _plane.Y * cos;
        }
        else if (keyboardState.IsKeyDown(Keys.D))
        {
            Vector2 oldDirection = _transform.Local.Direction;
            var cos = MathF.Cos(rotSpeed);
            var sin = MathF.Sin(rotSpeed);

            _transform.Local.Direction = new(
                oldDirection.X * cos - oldDirection.Y * sin,
                oldDirection.X * sin + oldDirection.Y * cos);

            Vector2 oldPlane = _plane;
            _plane.X = _plane.X * cos - _plane.Y * sin;
            _plane.Y = oldPlane.X * sin + _plane.Y * cos;
        }
    }

    private bool TryPickupNearbyItem()
    {
        if (_inventory == null || EntityContainer == null)
            return false;

        var playerPos = _transform.World.Position;

        foreach (var entity in EntityContainer.Children)
        {
            if (!entity.Enabled)
                continue;

            if (!entity.Components.TryGet<PickupableComponent>(out var pickupable))
                continue;

            if (!pickupable.IsInRange(playerPos))
                continue;

            var itemInstance = pickupable.CreateItemInstance();
            var result = _inventory.AddItem(itemInstance);

            if (result == AddItemResult.Success)
            {
                pickupable.OnPickedUp();
                return true;
            }
        }

        return false;
    }

    private bool TryOpenDoor()
    {
        float checkDistance = 1.5f;

        for (float dist = 0.1f; dist <= checkDistance; dist += 0.1f)
        {
            int checkX = (int)(_transform.World.Position.X + _transform.World.Direction.X * dist);
            int checkY = (int)(_transform.World.Position.Y + _transform.World.Direction.Y * dist);

            var door = _map.GetDoor(checkX, checkY);
            if (door is not null)
            {
                door.StartOpening();
                return true;
            }
        }

        return false;
    }

    //TODO: not sure I like this here. should be in the camera
    public Vector2 Plane => _plane;
}
