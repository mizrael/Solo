using Microsoft.Xna.Framework;

namespace Tetris;

public record Piece(int Id, PieceTemplate Template, Color Color)
{
    public Point Position { get; set; } = Point.Zero;

    private int _shapeIndex { get; set; }

    public Shape CurrentShape => Template.Shapes[_shapeIndex];

    public void Rotate()
    {
        _shapeIndex = (_shapeIndex + 1) % Template.Shapes.Length;
    }

    public override int GetHashCode()
    => this.Id.GetHashCode();
}
