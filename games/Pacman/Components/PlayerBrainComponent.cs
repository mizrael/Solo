using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Components;
using Solo.Services;
using System;

namespace Pacman.Components;

public class PlayerBrainComponent : Component
{
    private TransformComponent _transform;
    private MapLogicComponent _mapLogic;
    private TileInfo _currTile;
    private Directions _direction;

    public PlayerBrainComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _mapLogic = Map.Components.Get<MapLogicComponent>();

        _currTile = _mapLogic.GetPlayerStartTile();
        _transform = Owner.Components.Get<TransformComponent>();

        var renderer = Owner.Components.Get<AnimatedSpriteSheetRenderer>();
        var bbox = Owner.Components.Add<BoundingBoxComponent>();

        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();
        var calculateSize = new Action(() =>
        {
            if (renderer.CurrentFrame is null)
                return;

            _transform.Local.Scale.X = _mapLogic.TileSize.X / renderer.CurrentFrame.Bounds.Width;
            _transform.Local.Scale.Y = _mapLogic.TileSize.Y / renderer.CurrentFrame.Bounds.Height;

            _transform.Local.Position = _mapLogic.GetTileCenter(_currTile);

            var bboxSize = new Point(
                 (int)((float)renderer.CurrentFrame.Bounds.Size.X * _transform.Local.Scale.X),
                 (int)((float)renderer.CurrentFrame.Bounds.Size.Y * _transform.Local.Scale.Y));
            bbox.SetSize(bboxSize);
        });
        calculateSize();

        renderService.Window.ClientSizeChanged += (s, e) => calculateSize();

        base.InitCore();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();

        var nextRow = _currTile.Row;
        var nextCol = _currTile.Col;

        Directions newDirection = Directions.None;
        if (keyboard.IsKeyDown(Keys.Up))
        {
            newDirection = Directions.Up;
            nextRow -= 1;
        }
        else if (keyboard.IsKeyDown(Keys.Down))
        {
            newDirection = Directions.Down;
            nextRow += 1;
        }
        else if (keyboard.IsKeyDown(Keys.Left))
        {
            newDirection = Directions.Left;
            nextCol -= 1;
        }
        else if (keyboard.IsKeyDown(Keys.Right))
        {
            newDirection = Directions.Right;
            nextCol += 1;
        }

        if (newDirection == Directions.None)
            return;

        _direction = newDirection;

        var isTeleport = false;
        if (nextRow < 0)
        {
            isTeleport = true;
            nextRow = _mapLogic.RowsCount - 1;
        }
        else if (nextRow >= _mapLogic.RowsCount)
        {
            isTeleport = true;
            nextRow = 0;
        }
        if (nextCol < 0)
        {
            isTeleport = true;
            nextCol = _mapLogic.ColsCount - 1;
        }
        else if (nextCol >= _mapLogic.ColsCount)
        {
            isTeleport = true;
            nextCol = 0;
        }

        Vector2 newPos;
        var nextTile = _mapLogic.GetTileAt(nextRow, nextCol);
        if (nextTile?.IsWalkable == true)
        {
            newPos = _mapLogic.GetTileCenter(nextTile);
        }
        else
        {
            // if we can't move to the next tile, we should move to the center of the current tile
            // trying to avoid bouncing back and forth between two tiles
            newPos = _mapLogic.GetTileCenter(_currTile);
            newPos = _direction switch
            {
                Directions.Up => new Vector2(_transform.Local.Position.X, newPos.Y),
                Directions.Down => new Vector2(_transform.Local.Position.X, newPos.Y),
                Directions.Left => new Vector2(newPos.X, _transform.Local.Position.Y),
                Directions.Right => new Vector2(newPos.X, _transform.Local.Position.Y),
                _ => newPos
            };
        }
        
        _transform.Local.Position = isTeleport ? newPos : Vector2.Lerp(_transform.Local.Position, newPos, Speed);

        _currTile = _mapLogic.GetTileAt(_transform.Local.Position);

        _transform.Local.Rotation = _direction switch
        {
            Directions.Up => MathF.PI * 1.5f,
            Directions.Down => MathF.PI * .5f,
            Directions.Left => MathF.PI,
            Directions.Right => 0,
            _ => _transform.Local.Rotation
        };
    }

    public void Reset()
    {
        _currTile = _mapLogic.GetPlayerStartTile();
        _transform.Local.Position = _mapLogic.GetTileCenter(_currTile);

        _direction = Directions.Right;
    }

    public float Speed = .1f;

    public GameObject Map;

    public Directions Direction => _direction;
}
