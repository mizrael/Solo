using System;

namespace Tetris;

public sealed record GameState
{
    public uint Score { get; private set; }

    public void IncreaseScore()
    {
        Score += 100;
    }
}