using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Components;

namespace Snake.Components;

public class SnakeBrain : Component
{
    private double _lastMoveTime;
    private const double MoveInterval = 250;

    public SnakeBrain(GameObject owner) : base(owner)
    {
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        base.UpdateCore(gameTime);

        if (gameTime.TotalGameTime.TotalMilliseconds - _lastMoveTime < MoveInterval)
            return;

        _lastMoveTime = gameTime.TotalGameTime.TotalMilliseconds;

        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Up) && Snake.Direction != Direction.Down)
            Snake.Direction = Direction.Up;
        else if (keyboard.IsKeyDown(Keys.Down) && Snake.Direction != Direction.Up)
            Snake.Direction = Direction.Down;
        else if (keyboard.IsKeyDown(Keys.Left) && Snake.Direction != Direction.Right)
            Snake.Direction = Direction.Left;
        else if (keyboard.IsKeyDown(Keys.Right) && Snake.Direction != Direction.Left)
            Snake.Direction = Direction.Right;

        Snake.Move();
    }

    public Snake Snake { get; set; }
}