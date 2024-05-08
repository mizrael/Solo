using System;
using Microsoft.Xna.Framework;

namespace Snake;

public class Snake
{
    public Snake()
    {
        Tail = Head = new Segment();
    }

    public void Move()
    {
        var newHead = new Segment
        {
            Tile = Head.Tile,
            Direction = Direction,
        };

        switch (Direction)
        {
            case Direction.Up:
                newHead.Tile.Y--;
                break;
            case Direction.Down:
                newHead.Tile.Y++;
                break;
            case Direction.Left:
                newHead.Tile.X--;
                break;
            case Direction.Right:
                newHead.Tile.X++;
                break;
        }

        if(Tail.Prev is not null){
            Tail.Prev.Next = null;
            Tail = Tail.Prev;
        }

        newHead.Next = Head;
        Head.Prev = newHead;
        Head = newHead;
    }

    public void Reset()
    {
        Head = Tail = new Segment();
    }

    public Direction Direction;
    public Segment Head { get; private set; }
    public Segment Tail { get; private set; }

    public record Segment
    {
        public Point Tile;
        public Direction Direction;
        public Segment? Next = null;
        public Segment? Prev = null;
    }
}