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
    private StatsComponent? _stats;

    private readonly Map _map;
    private KeyboardState _previousKeyboardState;
    private MouseState _previousMouseState;

    public CharacterPanel? CharacterPanel { get; set; }
    public SpatialGrid? SpatialGrid { get; set; }
    public Raycaster? Raycaster { get; set; }
    public GameObject? MiniMapEntity { get; set; }

    public PlayerBrain(GameObject owner, Map map) : base(owner)
    {
        _map = map;
    }

    protected override void InitCore()
    {
        _transform = this.Owner.Components.Get<TransformComponent>();
        _inventory = this.Owner.Components.Get<InventoryComponent>();
        _stats = this.Owner.Components.Get<StatsComponent>();

        base.InitCore();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        float ms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        float moveSpeed = ms * .015f;
        float rotSpeed = ms * .005f;

        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        // Toggle character panel with Tab
        if (keyboardState.IsKeyDown(Keys.Tab) && !_previousKeyboardState.IsKeyDown(Keys.Tab))
        {
            CharacterPanel?.Toggle();
        }

        // Toggle minimap with M
        if (keyboardState.IsKeyDown(Keys.M) && !_previousKeyboardState.IsKeyDown(Keys.M))
        {
            if (MiniMapEntity != null)
                MiniMapEntity.Enabled = !MiniMapEntity.Enabled;
        }

        // Pick up items with left mouse click on hovered item
        if (mouseState.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            TryPickupClickedItem();
        }

        // Open doors with E key
        if (keyboardState.IsKeyDown(Keys.E) && !_previousKeyboardState.IsKeyDown(Keys.E))
        {
            TryOpenDoor();
        }

        _previousKeyboardState = keyboardState;
        _previousMouseState = mouseState;

        float moveAmount = 0;
        if (keyboardState.IsKeyDown(Keys.W))
            moveAmount = moveSpeed;
        else if (keyboardState.IsKeyDown(Keys.S))
            moveAmount = -moveSpeed;

        if (moveAmount != 0)
        {
            var previousPos = _transform.Local.Position;
            var moveStep = _transform.World.Direction * moveAmount;

            if (!_map.IsBlocked((int)(_transform.World.Position.X + moveStep.X), (int)_transform.World.Position.Y))
                _transform.Local.Position.X += moveStep.X;

            if (!_map.IsBlocked((int)_transform.World.Position.X, (int)(_transform.World.Position.Y + moveStep.Y)))
                _transform.Local.Position.Y += moveStep.Y;

            // Track actual distance walked
            var actualDistance = Vector2.Distance(previousPos, _transform.Local.Position);
            if (actualDistance > 0)
                _stats?.Metrics.RecordWalking(actualDistance);
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

    private bool TryPickupClickedItem()
    {
        if (_inventory == null || Raycaster == null)
            return false;

        var hoveredEntity = Raycaster.HoveredEntity;
        if (hoveredEntity == null)
            return false;

        if (!hoveredEntity.Components.TryGet<PickupableComponent>(out var pickupable))
            return false;

        // Check if player is in range to pick up
        var playerPos = _transform.World.Position;
        if (!pickupable.IsInRange(playerPos))
            return false;

        var itemInstance = pickupable.CreateItemInstance();
        var result = _inventory.AddItem(itemInstance);

        if (result == AddItemResult.Success)
        {
            pickupable.OnPickedUp();
            return true;
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
