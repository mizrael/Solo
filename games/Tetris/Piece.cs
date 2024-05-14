using Microsoft.Xna.Framework;

namespace Tetris;

public record Piece(PieceTemplate Template, Color Color)
{
    public Point Position { get; set; }
    public int ShapeIndex { get; set; }
}
