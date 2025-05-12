namespace Pacman;

public sealed record GameState
{
    public uint Score { get; private set; }

    public void IncreaseScore(uint amount)
    {
        Score += amount;
    }

    public void Reset()
    {
        Score = 0;
    }
}