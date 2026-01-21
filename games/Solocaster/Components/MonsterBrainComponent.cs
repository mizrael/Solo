using System;
using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solocaster.Monsters;

namespace Solocaster.Components;

public class MonsterBrainComponent : Component
{
    private TransformComponent _transform;
    private BillboardComponent _billboard;
    private AnimatedSpriteProvider _animationController;
    private GameObject _player;

    private string _currentState = "idle";
    private float _facingAngle;

    public MonsterTemplate Template { get; set; }

    public MonsterBrainComponent(GameObject owner) : base(owner)
    {
    }

    public void Initialize(AnimatedSpriteProvider animationController, GameObject player)
    {
        _animationController = animationController;
        _player = player;
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();
        _billboard = Owner.Components.Get<BillboardComponent>();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (_animationController == null || _player == null)
            return;

        UpdateDirectionToPlayer();
        _animationController.Update(gameTime);
    }

    private void UpdateDirectionToPlayer()
    {
        var playerTransform = _player.Components.Get<TransformComponent>();
        if (playerTransform == null)
            return;

        var toPlayer = playerTransform.World.Position - _transform.World.Position;
        var angleToPlayer = MathF.Atan2(toPlayer.Y, toPlayer.X);

        var relativeAngle = angleToPlayer - _facingAngle;

        // Normalize to -PI to PI
        while (relativeAngle > MathF.PI) relativeAngle -= MathF.PI * 2;
        while (relativeAngle < -MathF.PI) relativeAngle += MathF.PI * 2;

        // Map to direction (from monster's perspective, so inverted)
        // If player is in front of monster, monster shows its front to player
        var direction = relativeAngle switch
        {
            >= -MathF.PI / 4 and < MathF.PI / 4 => Direction.Front,
            >= MathF.PI / 4 and < 3 * MathF.PI / 4 => Direction.Right,
            >= -3 * MathF.PI / 4 and < -MathF.PI / 4 => Direction.Left,
            _ => Direction.Back
        };

        _animationController.SetDirection(direction);
    }

    public void SetState(string state)
    {
        _currentState = state;
        _animationController?.SetState(state);
    }

    public string CurrentState => _currentState;
}
