using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Components;
using Solo.Services;
using Tetris.Scenes;

namespace Tetris.Components;

public class PieceController : Component
{
    private Piece? _currPiece;

    private double _lastInputUpdate;
    private double _inputUpdateInterval = 200;

    private double _lastGravityUpdate;
    private double _gravityInterval = 500;
    private double _startGravityInterval = 500;
    private double _maxGravityInterval = 50;

    public PieceController(GameObject owner) : base(owner)
    {
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        HandleInput(gameTime);

        HandleGravity(gameTime);

        base.UpdateCore(gameTime);
    }

    private void HandleInput(GameTime gameTime)
    {
        if (_currPiece is null)
            return;

        var elapsed = gameTime.TotalGameTime.TotalMilliseconds - _lastInputUpdate;
        if (elapsed < _inputUpdateInterval)
            return;
        _lastInputUpdate = gameTime.TotalGameTime.TotalMilliseconds;

        var keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.Left))
        {
            var nextPos = _currPiece.Position + new Point(-1, 0);
            if (Board.CanPlace(_currPiece, nextPos))
                _currPiece.Position = nextPos;
        }
        else if (keyboard.IsKeyDown(Keys.Right))
        {
            var nextPos = _currPiece.Position + new Point(1, 0);
            if (Board.CanPlace(_currPiece, nextPos))
                _currPiece.Position = nextPos;
        }
        
        if (keyboard.IsKeyDown(Keys.Up))
        {
            _currPiece.Rotate();
        }

        if (keyboard.IsKeyDown(Keys.Down))
        {
            _gravityInterval -= 100;
            if (_gravityInterval < _maxGravityInterval)
                _gravityInterval = _maxGravityInterval;
        }
    }

    private void HandleGravity(GameTime gameTime)
    {
        var elapsed = gameTime.TotalGameTime.TotalMilliseconds - _lastGravityUpdate;
        if (elapsed < _gravityInterval)
            return;
        _lastGravityUpdate = gameTime.TotalGameTime.TotalMilliseconds;
        
        Point nextPos;
        if (_currPiece is null)
        {
            _currPiece = Generator.Step();

            var halfSize = _currPiece.CurrentShape.Tiles.GetLength(0) / 2;
            nextPos = new Point(Board.Width / 2 - halfSize, 0);

            _gravityInterval = _startGravityInterval;
        }
        else
            nextPos = _currPiece.Position + new Point(0, 1);

        if (Board.CanPlace(_currPiece, nextPos))
        {
            _currPiece.Position = nextPos;
            Board.Place(_currPiece);
        }
        else
        {
            if (Board.CheckGameover())
            {
                SceneManager.Instance.SetScene(SceneNames.Play);
                return;
            }

            _currPiece = null; 
        }
    }

    public Board Board;
    public PieceGenerator Generator;
}