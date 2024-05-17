using Microsoft.Xna.Framework;

namespace Tetris;

public record Piece(PieceTemplate Template, Color Color)
{
    public Point OriginTile { get; set; }
    public int ShapeIndex { get; set; }
}
