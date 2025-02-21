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
    private int _currRow = -1;
    private int _currCol = -1;
    private Directions _direction;


    public PlayerBrainComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _mapLogic = Map.Components.Get<MapLogicComponent>();

        (_currRow, _currCol) = _mapLogic.GetPlayerStartTile();
        _transform = Owner.Components.Get<TransformComponent>();
        _transform.Local.Position = _mapLogic.GetTileCenter(_currRow, _currCol);

        var renderer = Owner.Components.Get<AnimatedSpriteSheetRenderer>();

        var renderService = GameServicesManager.Instance.GetService<RenderService>();
        var calculateSize = new Action(() =>
        {
            if (renderer.CurrentFrame is null)
                return;

            _transform.Local.Scale.X = _mapLogic.TileSize.X / renderer.CurrentFrame.Bounds.Width;
            _transform.Local.Scale.Y = _mapLogic.TileSize.Y / renderer.CurrentFrame.Bounds.Height;

            _transform.Local.Position = _mapLogic.GetTileCenter(_currRow, _currCol);
        });
        calculateSize();

        renderService.Window.ClientSizeChanged += (s, e) => calculateSize();

        base.InitCore();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();

        var nextRow = _currRow;
        var nextCol = _currCol;

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
            nextRow = _mapLogic.Rows - 1;
        }
        else if (nextRow >= _mapLogic.Rows)
        {
            isTeleport = true;
            nextRow = 0;
        }
        if (nextCol < 0)
        {
            isTeleport = true;
            nextCol = _mapLogic.Cols - 1;
        }
        else if (nextCol >= _mapLogic.Cols)
        {
            isTeleport = true;
            nextCol = 0;
        }

        Vector2 newPos;
        if (_mapLogic.IsWalkable(nextRow, nextCol))
        {
            newPos = _mapLogic.GetTileCenter(nextRow, nextCol);
        }
        else
        {
            // if we can't move to the next tile, we should move to the center of the current tile
            // trying to avoid bouncing back and forth between two tiles
            newPos = _mapLogic.GetTileCenter(_currRow, _currCol);
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

        (_currRow, _currCol) = _mapLogic.GetTileIndex(_transform.Local.Position);

        _transform.Local.Rotation = _direction switch
        {
            Directions.Up => MathF.PI * 1.5f,
            Directions.Down => MathF.PI * .5f,
            Directions.Left => MathF.PI,
            Directions.Right => 0,
            _ => _transform.Local.Rotation
        };
    }

    public float Speed = .1f;

    public GameObject Map;
}

public enum Directions
{
    None,
    Up,
    Down,
    Left,
    Right
}