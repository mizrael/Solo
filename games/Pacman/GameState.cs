namespace Pacman;

public sealed record GameState
{
    public uint Score { get; private set; }

    public void IncreaseScore(uint amount)
    {
        Score += amount;
    }
}