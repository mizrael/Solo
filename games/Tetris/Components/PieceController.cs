using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;

namespace Tetris.Components;

public class PieceController : Component
{
    private Piece? _currPiece;
    private double _lastUpdate;
    private double _updateInterval = 500;

    public PieceController(GameObject owner) : base(owner)
    {
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        var elapsed = gameTime.TotalGameTime.TotalMilliseconds - _lastUpdate;
        if (elapsed < _updateInterval)
            return;

        _lastUpdate = gameTime.TotalGameTime.TotalMilliseconds;

        var nextPos = Point.Zero;
        if (_currPiece is null)
            _currPiece = Generator.Create();
        else
            nextPos = _currPiece.Position + new Point(0, 1);

        if (Board.CanPlace(_currPiece, nextPos))
        {
            _currPiece.Position = nextPos;
            Board.Place(_currPiece);
        }
        else
        {
            _currPiece = null;
        }


        // Point nextPos = Point.Zero;
        // var needUpdate = false;

        // if (_currPiece == null)
        // {
        //     _currPiece = Generator.Create();
        //     nextPos = _currPiece.Position;
        //     needUpdate = true;
        // }
        // else
        // {
        //     var elapsed = gameTime.TotalGameTime.TotalMilliseconds - _lastUpdate;
        //     if (elapsed >= _updateInterval)
        //     {
        //         _lastUpdate = gameTime.TotalGameTime.TotalMilliseconds;
        //         nextPos = _currPiece.Position + new Point(0, 1);
        //         needUpdate = true;
        //     }
        // }

        // if (needUpdate)
        // {
        //     if (Board.CanPlace(_currPiece, nextPos))
        //     {
        //         _currPiece.Position = nextPos;
        //         Board.Place(_currPiece);
        //     }
        //     else
        //     {
        //         _currPiece = null;
        //     }
        // }

        base.UpdateCore(gameTime);
    }

    public Board Board;
    public PieceGenerator Generator;
}