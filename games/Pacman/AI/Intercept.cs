using Microsoft.Xna.Framework;
using Pacman.Components;
using Solo;
using Solo.Components;

namespace Pacman.AI;

public record Intercept : Chase
{
    private TransformComponent _targetTransform;
    private MapLogicComponent _mapLogic;
    private readonly float _offset;

    public Intercept(
         GameObject owner,
         GameObject target,
         GameObject map,
         float offset) : base(owner, target, map)
    {
        _offset = offset;
    }

    protected override void OnEnter()
    {
        base.OnEnter();
        _targetTransform = Target.Components.Get<TransformComponent>();
        _mapLogic = Map.Components.Get<MapLogicComponent>();
    }

    protected override TileInfo FindTargetTile()
    {
        var targetPos = _targetTransform.World.Position;
        var offset = Vector2.One * _targetTransform.World.Rotation * _offset;

        var dest = targetPos + offset;
        var destTile = _mapLogic.GetTileAt(dest) ?? _mapLogic.GetTileAt(targetPos);
        return destTile;
    }
}