using Microsoft.Xna.Framework;
using Pacman.Components;
using Solo;
using Solo.AI;
using Solo.Components;

namespace Pacman.AI;

public record Arrive : State
{
    private Path<TileInfo>? _path;
    private TileInfo? _currPathNode;
    private TileInfo _targetTile;
    private MapLogicComponent _mapLogic;
    private TransformComponent _ownerTransform;

    public Arrive(
        GameObject owner,
        TileInfo targetTile,
        GameObject map) : base(owner)
    {
        _targetTile = targetTile;
        Map = map;
    }

    protected override void OnEnter(Game game)
    {
        _mapLogic = Map.Components.Get<MapLogicComponent>();
        _ownerTransform = Owner.Components.Get<TransformComponent>();
        _path = null;
        _currPathNode = null;

        this.Owner.Components.Get<GhostBrainComponent>().SetAnimation(GhostAnimations.Walk);
    }

    protected override void OnExecute(Game game, GameTime gameTime)
    {
        var currTile = _mapLogic.GetTileAt(_ownerTransform.World.Position);

        _path ??= _mapLogic.FindPath(currTile, _targetTile);

        if (!_path.Any() && _currPathNode is null)
            return;

        _currPathNode ??= _path.Next();

        if (currTile == _currPathNode)
        {
            _currPathNode = null;
            if (!_path.Any())
                _path = null;
        }
        else
        {
            var tilePos = _mapLogic.GetTileCenter(_currPathNode);
            var newPos = Vector2.Lerp(_ownerTransform.World.Position, tilePos, Speed);
            _ownerTransform.Local.Position = newPos;
        }

        base.OnExecute(game, gameTime);
    }

    public GameObject Map { get; }

    public float Speed = .065f;
}