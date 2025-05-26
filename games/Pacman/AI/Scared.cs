using Microsoft.Xna.Framework;
using Pacman.Components;
using Solo;
using Solo.AI;
using Solo.Components;
using System;

namespace Pacman.AI;

public record Scared : State
{
    private readonly GameObject _map;
    private readonly float _durationMs;
    private readonly float _threshold;
    private bool _isAlmostDone = false;

    private MapLogicComponent _mapLogic;
    private TransformComponent _ownerTransform;

    private TileInfo? _destTile;
    private Directions? _currDir;

    private static readonly Directions[] _directions = Enum.GetValues<Directions>();

    public Scared(GameObject owner, GameObject map, float durationMs = 1000 * 10) : base(owner)
    {
        _map = map;
        _durationMs = durationMs;
        _threshold = durationMs * 0.75f;
    }

    protected override void OnEnter(Game game)
    {
        _isAlmostDone = false;
        _destTile = null;
        _mapLogic = _map.Components.Get<MapLogicComponent>();
        _ownerTransform = Owner.Components.Get<TransformComponent>();

        this.Owner.Components.Get<GhostBrainComponent>().SetAnimation(GhostAnimations.Scared1, game);
    }

    protected override void OnExecute(Game game, GameTime gameTime)
    {
        if (ElapsedMilliseconds > _durationMs)
        {
            IsCompleted = true;
            return;
        }
        else if (ElapsedMilliseconds > _threshold && !_isAlmostDone)
        {
            _isAlmostDone = true;
            this.Owner.Components.Get<GhostBrainComponent>().SetAnimation(GhostAnimations.Scared2, game);
        }

        var currTile = _mapLogic.GetTileAt(_ownerTransform.World.Position);
        if (_destTile == currTile || _destTile is null)
        {
            _destTile = null;
            var start = Random.Shared.Next(0, _directions.Length);
            var end = start + _directions.Length;
            for (int i = start; i != end; i++)
            {
                var dir = _directions[i % _directions.Length];
                if (_currDir is not null && IsOpposite(_currDir.Value, dir))
                    continue;

                var nextTilePos = currTile.Add(1, dir);
                var nextTile = _mapLogic.GetTileAt(nextTilePos);
                if (nextTile is not null && nextTile.IsWalkable && nextTile != currTile)
                {
                    _currDir = dir; 
                    _destTile = nextTile; 
                    break;
                }
            }
        }

        if (_destTile is null) 
            return;

        var tilePos = _mapLogic.GetTileCenter(_destTile);
        var newPos = Vector2.Lerp(_ownerTransform.World.Position, tilePos, Speed);
        _ownerTransform.Local.Position = newPos;

        base.OnExecute(game, gameTime);
    }

    private bool IsOpposite(Directions dir1, Directions dir2)
    => (dir1 == Directions.Up && dir2 == Directions.Down) ||
        (dir1 == Directions.Down && dir2 == Directions.Up) ||
        (dir1 == Directions.Left && dir2 == Directions.Right) ||
        (dir1 == Directions.Right && dir2 == Directions.Left);

    protected override void OnExit(Game game)
    {
        var brain = Owner.Components.Get<GhostBrainComponent>();
        brain.IsScared = false;

        base.OnExit(game);
    }

    public float Speed = .085f;
}