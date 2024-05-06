using Microsoft.Xna.Framework;

namespace Snake;

public class Snake
{
    public Snake()
    {
        Tail = Head = new Segment();
    }

    public Direction Direction;

    public readonly Segment Head;
    public readonly Segment Tail;

    public record Segment
    {
        public Point Tile;
        public Direction Direction;
        public Segment? Next = null;
        public Segment? Prev = null;
    }
}