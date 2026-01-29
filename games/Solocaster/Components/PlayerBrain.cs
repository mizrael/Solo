using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.AI;
using Solo.Components;
using Solocaster.AI.Player;
using Solocaster.Entities;
using Solocaster.Services;
using System;

namespace Solocaster.Components;

public class PlayerBrain : Component
{
    private readonly Map _map;
    private readonly InputService _inputService;

    private TransformComponent _transform = null!;
    private InventoryComponent _inventory = null!;
    private StatsComponent _stats = null!;

    private StateMachine _stateMachine = null!;
    private PlayerStateContext _context = null!;

    private PlayerExploringState _exploringState = null!;
    private PlayerCombatState _combatState = null!;
    private PlayerRunningState _runningState = null!;
    private PlayerExhaustedState _exhaustedState = null!;

    private MouseState _previousMouseState;

    public Raycaster? Raycaster { get; set; }

    public Vector2 Plane { get; private set; } = new Vector2(0, 0.45f);

    public bool ShowsHands => _context?.ShowsHands ?? false;
    public float BobSpeed => _context?.BobSpeed ?? 1.5f;

    public float LeftHandRaiseAmount => _context?.LeftHandRaiseAmount ?? 0f;
    public float RightHandRaiseAmount => _context?.RightHandRaiseAmount ?? 0f;

    public PlayerBrain(GameObject owner, Map map, InputService inputService) : base(owner)
    {
        _map = map;
        _inputService = inputService;
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();
        _inventory = Owner.Components.Get<InventoryComponent>();
        _stats = Owner.Components.Get<StatsComponent>();

        _context = new PlayerStateContext
        {
            Inventory = _inventory,
            Stats = _stats,
        };

        _exploringState = new PlayerExploringState(Owner, _context);
        _combatState = new PlayerCombatState(Owner, _context);
        _runningState = new PlayerRunningState(Owner, _context);
        _exhaustedState = new PlayerExhaustedState(Owner, _context);

        _stateMachine = new StateMachine(_exploringState);
        SetupTransitions();

        base.InitCore();
    }

    private void SetupTransitions()
    {
        // Exploring -> Combat (R key)
        _stateMachine.AddTransition(_exploringState, _combatState,
            _ => _inputService.IsActionPressed(InputActions.ToggleCombat));

        // Combat -> Exploring (R key)
        _stateMachine.AddTransition(_combatState, _exploringState,
            _ => _inputService.IsActionPressed(InputActions.ToggleCombat));

        // Exploring -> Running (Shift + Forward + has stamina)
        _stateMachine.AddTransition(_exploringState, _runningState,
            _ => _inputService.IsActionDown(InputActions.Run) &&
                 _inputService.IsActionDown(InputActions.MoveForward) &&
                 _stats.CurrentStamina > 0,
            _ => _context.StateBeforeRun = _exploringState);

        // Combat -> Running (Shift + Forward + has stamina)
        _stateMachine.AddTransition(_combatState, _runningState,
            _ => _inputService.IsActionDown(InputActions.Run) &&
                 _inputService.IsActionDown(InputActions.MoveForward) &&
                 _stats.CurrentStamina > 0,
            _ => _context.StateBeforeRun = _combatState);

        // Running -> Exhausted (stamina depleted)
        _stateMachine.AddTransition(_runningState, _exhaustedState,
            _ => _stats.IsExhausted);

        // Running -> Exploring (pressed backward - always to exploring)
        _stateMachine.AddTransition(_runningState, _exploringState,
            _ => _inputService.IsActionDown(InputActions.MoveBackward));

        // Running -> Exploring (stopped running, was exploring)
        _stateMachine.AddTransition(_runningState, _exploringState,
            _ => (!_inputService.IsActionDown(InputActions.Run) ||
                  !_inputService.IsActionDown(InputActions.MoveForward)) &&
                 _context.StateBeforeRun == _exploringState);

        // Running -> Combat (stopped running, was in combat)
        _stateMachine.AddTransition(_runningState, _combatState,
            _ => (!_inputService.IsActionDown(InputActions.Run) ||
                  !_inputService.IsActionDown(InputActions.MoveForward)) &&
                 _context.StateBeforeRun == _combatState);

        // Exhausted -> Exploring (recovered, was exploring)
        _stateMachine.AddTransition(_exhaustedState, _exploringState,
            _ => !_stats.IsExhausted && _context.StateBeforeRun == _exploringState);

        // Exhausted -> Combat (recovered, was in combat)
        _stateMachine.AddTransition(_exhaustedState, _combatState,
            _ => !_stats.IsExhausted && _context.StateBeforeRun == _combatState);
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        float ms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        float baseSpeed = ms * 0.005f;
        float rotSpeed = ms * 0.005f;

        var mouseState = Mouse.GetState();

        // Handle interactions (available in all states)
        HandlePickup(mouseState);
        HandleInteract();

        // Update state machine
        _stateMachine.Update(gameTime);

        // Handle movement (after state machine sets speed multiplier)
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        HandleMovement(baseSpeed, deltaTime);
        HandleRotation(rotSpeed);

        // Update previous mouse state AFTER state machine so combat can detect clicks
        _previousMouseState = mouseState;
        _context.PreviousMouseState = mouseState;
    }

    private void HandlePickup(MouseState mouseState)
    {
        if (mouseState.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            TryPickupClickedItem();
        }
    }

    private bool TryPickupClickedItem()
    {
        if (Raycaster == null)
            return false;

        var hoveredEntity = Raycaster.HoveredEntity;
        if (hoveredEntity == null)
            return false;

        if (!hoveredEntity.Components.TryGet<PickupableComponent>(out var pickupable))
            return false;

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

    private void HandleInteract()
    {
        if (_inputService.IsActionPressed(InputActions.Interact))
            TryOpenDoor();
    }

    private bool TryOpenDoor()
    {
        const float checkDistance = 1.5f;

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

    private void HandleMovement(float baseSpeed, float deltaTime)
    {
        float moveSpeed = baseSpeed * _context.SpeedMultiplier;
        float moveAmount = 0f;

        if (_inputService.IsActionDown(InputActions.MoveForward))
            moveAmount = moveSpeed;
        else if (_inputService.IsActionDown(InputActions.MoveBackward))
            moveAmount = -moveSpeed;

        _context.CurrentMoveSpeed = MathF.Abs(moveAmount);

        if (moveAmount == 0)
            return;

        var previousPos = _transform.Local.Position;
        var moveStep = _transform.World.Direction * moveAmount;

        if (!_map.IsBlocked((int)(_transform.World.Position.X + moveStep.X), (int)_transform.World.Position.Y))
            _transform.Local.Position.X += moveStep.X;

        if (!_map.IsBlocked((int)_transform.World.Position.X, (int)(_transform.World.Position.Y + moveStep.Y)))
            _transform.Local.Position.Y += moveStep.Y;

        var actualDistance = Vector2.Distance(previousPos, _transform.Local.Position);
        if (actualDistance > 0)
        {
            if (_stateMachine.CurrentState is PlayerRunningState)
                _stats.Metrics.RecordRunning(actualDistance, deltaTime);
            else
                _stats.Metrics.RecordWalking(actualDistance, deltaTime);
        }
    }

    private void HandleRotation(float rotSpeed)
    {
        if (_inputService.IsActionDown(InputActions.RotateLeft))
            RotatePlayer(-rotSpeed);
        else if (_inputService.IsActionDown(InputActions.RotateRight))
            RotatePlayer(rotSpeed);
    }

    private void RotatePlayer(float angle)
    {
        var oldDirection = _transform.Local.Direction;
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);

        _transform.Local.Direction = new Vector2(
            oldDirection.X * cos - oldDirection.Y * sin,
            oldDirection.X * sin + oldDirection.Y * cos
        );

        var oldPlane = Plane;
        Plane = new Vector2(
            oldPlane.X * cos - oldPlane.Y * sin,
            oldPlane.X * sin + oldPlane.Y * cos
        );
    }
}
