using Pacman.Components;
using Pacman.Scenes;
using Solo;
using Solo.Components;
using System;

namespace Pacman.AI;

public record InkyIntercept : Chase
{
    private TransformComponent _playerTransform;
    private MapLogicComponent _mapLogic;
    private readonly PlayScene _playScene;
    private TransformComponent _blinkyTransform;

    public InkyIntercept(
         GameObject owner,
         GameObject player,
         GameObject map,
         Scenes.PlayScene playScene) : base(owner, player, map)
    {
        _playScene = playScene;
    }

    protected override void OnEnter()
    {
        base.OnEnter();

        _playerTransform = Target.Components.Get<TransformComponent>();
        _mapLogic = Map.Components.Get<MapLogicComponent>();

        var blinky = _playScene.Root.FindFirst(o => o.HasTag(GhostTypes.Blinky.ToString().ToLower()));
        if(blinky is null)
            throw new InvalidOperationException("Blinky ghost not found in the scene.");
        _blinkyTransform = blinky.Components.Get<TransformComponent>();           
    }

    protected override TileInfo FindTargetTile()
    {
        var playerPos = _playerTransform.World.Position;
        var playerDir = this.Target.Components.Get<PlayerBrainComponent>().Direction;
        var playerTile = _mapLogic.GetTileAt(playerPos)!;

        var targetTilePos = playerTile.Add(2, playerDir);

        if (_blinkyTransform is not null)
        {
            var blinkyTile = _mapLogic.GetTileAt(_blinkyTransform.World.Position);
            var vec = blinkyTile + (targetTilePos - blinkyTile) * 2;
            var bt = _mapLogic.GetTileAt(vec);
            if (bt is not null && bt.IsWalkable)
                return bt;
        }

        var targetTile = _mapLogic.GetTileAt(targetTilePos);
        return (targetTile is not null && targetTile.IsWalkable) ? targetTile : playerTile;
    }
}