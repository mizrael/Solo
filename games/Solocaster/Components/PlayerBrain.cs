using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Components;
using Solocaster.Entities;
using Solocaster.Input;
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
    private MouseState _previousMouseState;

    private PlayerState _state = PlayerState.Exploring;
    private PlayerState _previousStateBeforeRun = PlayerState.Exploring;
    private InputBindings? _inputBindings;

    private const float RunningSpeedMultiplier = 1.8f;
    private const float ExhaustedSpeedMultiplier = 0.6f;

    public CharacterPanel? CharacterPanel { get; set; }
    public MetricsPanel? MetricsPanel { get; set; }
    public SpatialGrid? SpatialGrid { get; set; }
    public Raycaster? Raycaster { get; set; }
    public GameObject? MiniMapEntity { get; set; }

    //TODO: not sure I like this here. should be in the camera
    public Vector2 Plane => _plane;

    public PlayerState State => _state;
    public InputBindings? InputBindings => _inputBindings;

    public GameObject? DebugUIEntity { get; set; }

    public float CurrentMoveSpeed { get; private set; }

    public PlayerBrain(GameObject owner, Map map) : base(owner)
    {
        _map = map;
    }

    protected override void InitCore()
    {
        _transform = this.Owner.Components.Get<TransformComponent>();
        _inventory = this.Owner.Components.Get<InventoryComponent>();
        _stats = this.Owner.Components.Get<StatsComponent>();

        _inputBindings = new InputBindings();

        base.InitCore();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        _inputBindings?.Update();

        float ms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        float moveSpeed = ms * .005f;
        float rotSpeed = ms * .005f;

        var mouseState = Mouse.GetState();

        // Toggle character panel
        if (_inputBindings?.IsActionPressed(InputActions.ToggleCharacterPanel) == true)
        {
            CharacterPanel?.Toggle();
        }

        // Toggle minimap
        if (_inputBindings?.IsActionPressed(InputActions.ToggleMinimap) == true)
        {
            if (MiniMapEntity != null)
                MiniMapEntity.Enabled = !MiniMapEntity.Enabled;
        }

        // Toggle debug UI
        if (_inputBindings?.IsActionPressed(InputActions.ToggleDebug) == true)
        {
            if (DebugUIEntity != null)
                DebugUIEntity.Enabled = !DebugUIEntity.Enabled;
        }

        // Toggle metrics panel
        if (_inputBindings?.IsActionPressed(InputActions.ToggleMetrics) == true)
        {
            MetricsPanel?.Toggle();
        }

        // Pick up items with left mouse click on hovered item
        if (mouseState.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            TryPickupClickedItem();
        }

        // Open doors / interact
        if (_inputBindings?.IsActionPressed(InputActions.Interact) == true)
        {
            TryOpenDoor();
        }

        // Toggle combat mode
        if (_inputBindings?.IsActionPressed(InputActions.ToggleCombat) == true &&
            _state != PlayerState.Running && _state != PlayerState.Exhausted)
        {
            _state = _state == PlayerState.Combat ? PlayerState.Exploring : PlayerState.Combat;
        }

        _previousMouseState = mouseState;

        float moveAmount = 0;
        bool wantsToRun = _inputBindings?.IsActionDown(InputActions.Run) == true;
        bool movingForward = _inputBindings?.IsActionDown(InputActions.MoveForward) == true;
        bool movingBackward = _inputBindings?.IsActionDown(InputActions.MoveBackward) == true;

        if (movingForward)
            moveAmount = moveSpeed;
        else if (movingBackward)
            moveAmount = -moveSpeed;

        // Handle running state
        if (_state == PlayerState.Exhausted)
        {
            moveAmount *= ExhaustedSpeedMultiplier;
            _stats?.UpdateStamina((float)gameTime.ElapsedGameTime.TotalSeconds, false);

            if (!_stats?.IsExhausted ?? true)
            {
                _state = _previousStateBeforeRun;
            }
        }
        else if (wantsToRun && movingForward && _stats?.CurrentStamina > 0 && _state != PlayerState.Exhausted)
        {
            if (_state != PlayerState.Running)
            {
                _previousStateBeforeRun = _state;
                _state = PlayerState.Running;
            }

            moveAmount *= RunningSpeedMultiplier;
            _stats?.DrainStamina((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (_stats?.IsExhausted ?? false)
            {
                _state = PlayerState.Exhausted;
            }
        }
        else
        {
            if (_state == PlayerState.Running)
            {
                _state = _previousStateBeforeRun;
            }
            _stats?.UpdateStamina((float)gameTime.ElapsedGameTime.TotalSeconds, false);
        }

        CurrentMoveSpeed = MathF.Abs(moveAmount);

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

        if (_inputBindings?.IsActionDown(InputActions.RotateLeft) == true)
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
        else if (_inputBindings?.IsActionDown(InputActions.RotateRight) == true)
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
}
