using System;
using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solocaster.Monsters;

namespace Solocaster.Components;

public class MonsterBrainComponent : Component
{
    private TransformComponent _transform;
    private IDirectionalFrameProvider _spriteProvider;
    private GameObject _player;

    private const float FacingAngle = 0f;

    public MonsterTemplate Template { get; set; }

    public MonsterBrainComponent(GameObject owner) : base(owner)
    {
    }

    public void Initialize(IDirectionalFrameProvider spriteProvider, GameObject player)
    {
        _spriteProvider = spriteProvider;
        _player = player;
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (_spriteProvider == null || _player == null)
            return;

        UpdateDirectionToPlayer();
    }

    private void UpdateDirectionToPlayer()
    {
        var playerTransform = _player.Components.Get<TransformComponent>();
        if (playerTransform == null)
            return;

        var toPlayer = playerTransform.World.Position - _transform.World.Position;
        var angleToPlayer = MathF.Atan2(toPlayer.Y, toPlayer.X);

        var relativeAngle = angleToPlayer - FacingAngle;

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

        _spriteProvider.SetDirection(direction);
    }
}
