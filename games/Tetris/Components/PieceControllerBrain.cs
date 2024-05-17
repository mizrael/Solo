using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;

namespace Tetris.Components;

public class PieceControllerBrain : Component
{
    private Piece? _currPiece;

    public PieceControllerBrain(GameObject owner) : base(owner)
    {
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (_currPiece == null)
        {
            _currPiece = Generator.Create();
            Board.SetPiece(_currPiece);
        }

        base.UpdateCore(gameTime);
    }

    public Board Board;
    public PieceGenerator Generator;
}