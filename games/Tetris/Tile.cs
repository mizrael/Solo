namespace Tetris;

public record Tile
{
    private static int _nextId = 0;
    public readonly int Id = ++_nextId;
    public bool IsFilled => Piece is not null;
    public Piece? Piece;

    public override int GetHashCode()
    => this.Id.GetHashCode();
}
