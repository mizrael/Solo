using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Components;

namespace Snake.Components;

public class SnakeBrain : Component
{
    private double _lastMoveTime;
    private const double MoveInterval = 150;

    public SnakeBrain(GameObject owner) : base(owner)
    {
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        base.UpdateCore(gameTime);

        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Up) && Snake.Direction != Direction.Down)
            Snake.Direction = Direction.Up;
        else if (keyboard.IsKeyDown(Keys.Down) && Snake.Direction != Direction.Up)
            Snake.Direction = Direction.Down;
        else if (keyboard.IsKeyDown(Keys.Left) && Snake.Direction != Direction.Right)
            Snake.Direction = Direction.Left;
        else if (keyboard.IsKeyDown(Keys.Right) && Snake.Direction != Direction.Left)
            Snake.Direction = Direction.Right;

        if (gameTime.TotalGameTime.TotalMilliseconds - _lastMoveTime < MoveInterval)
            return;

        _lastMoveTime = gameTime.TotalGameTime.TotalMilliseconds;

        Snake.Move();

        if (Snake.Head.Tile.X < 0 || Snake.Head.Tile.X >= Board.Width ||
            Snake.Head.Tile.Y < 0 || Snake.Head.Tile.Y >= Board.Height)            
        {
            OnDeath?.Invoke();
            return;
        }

        if(Snake.CheckSelfHit())
        {
            OnDeath?.Invoke();
            return;
        }

        var tile = Board.GetTileAt(Snake.Head.Tile);
        if (tile == TileType.Wall)
        {
            OnDeath?.Invoke();
        }
        else if(tile == TileType.Food)
        {
            Snake.Eat(Snake.Head.Tile, Board);
        }
    }

    public delegate void OnDeathHandler();
    public event OnDeathHandler OnDeath;

    public Snake Snake { get; set; }
    public Board Board { get; set; }
}