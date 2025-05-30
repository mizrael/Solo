using Microsoft.Xna.Framework;
using Pacman.Components;
using Solo;
using Solo.Components;

namespace Pacman.AI;

public record Intercept : Chase
{
    private TransformComponent _playerTransform;
    private MapLogicComponent _mapLogic;

    public Intercept(
         GameObject owner,
         GameObject target,
         GameObject map) : base(owner, target, map)
    {
    }

    protected override void OnEnter(Game game)
    {
        base.OnEnter(game);
        _playerTransform = Target.Components.Get<TransformComponent>();
        _mapLogic = Map.Components.Get<MapLogicComponent>();
    }

    protected override TileInfo FindTargetTile()
    {
        var playerPos = _playerTransform.World.Position;
        var playerDir = this.Target.Components.Get<PlayerBrainComponent>().Direction;
        var playerTile = _mapLogic.GetTileAt(playerPos)!;

        var targetTilePos = playerTile.Add(2, playerDir);

        var targetTile = _mapLogic.GetTileAt(targetTilePos);
        return (targetTile is not null && targetTile.IsWalkable) ? targetTile : playerTile;
    }
}
