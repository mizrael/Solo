using Microsoft.Xna.Framework;
using Pacman.Components;
using Solo;
using Solo.AI;
using Solo.Components;

namespace Pacman.AI;

public record Chase : State
{
    private Path<TileInfo>? _path;
    private TileInfo? _currPathNode;
    private TransformComponent _targetTransform;
    private MapLogicComponent _mapLogic;
    private TransformComponent _ownerTransform;

    public Chase(
        GameObject owner, 
        GameObject target, 
        GameObject map) : base(owner)
    {
        Target = target;
        Map = map;
    }

    protected override void OnEnter(Game game)
    {
        _mapLogic = Map.Components.Get<MapLogicComponent>();
        _targetTransform = Target.Components.Get<TransformComponent>();
        _ownerTransform = Owner.Components.Get<TransformComponent>();
        _path = null;
        _currPathNode = null;

        this.Owner.Components.Get<GhostBrainComponent>().SetAnimation(GhostAnimations.Walk, game);
    }

    protected override void OnExecute(Game game, GameTime gameTime)
    {
        var currTile = _mapLogic.GetTileAt(_ownerTransform.World.Position);

        var targetCurrTile = FindTargetTile();

        _path ??= _mapLogic.FindPath(currTile, targetCurrTile);

        if (!_path.Any() && _currPathNode is null)
            return;

        var shouldRecalcPath = TileInfo.Distance(targetCurrTile, _path.End) > PathRecalcThreshold;
        if (shouldRecalcPath)
        {
            _path = null;
            _currPathNode = null;
            return;
        }

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

    protected virtual TileInfo FindTargetTile()
    => _mapLogic.GetTileAt(_targetTransform.World.Position);

    public GameObject Target { get; }
    public GameObject Map { get; }

    public float Speed = .065f;
    public float PathRecalcThreshold = 4f;
}
