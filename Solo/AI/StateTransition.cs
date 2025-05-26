namespace Solo.AI;

public interface IStateTransition
{
    bool CanTransition();
    void BeforeTransition();

    State To { get; }
}

public record StateTransition<TS1, TS2> : IStateTransition
    where TS1 : State
    where TS2 : State
{
    private readonly TS1 _from;
    public readonly TS2 _to;
    private readonly Predicate<TS1> _predicate;
    private readonly Action<TS2>? _beforeTransition;

    public StateTransition(TS1 from, TS2 to, Predicate<TS1> predicate, Action<TS2>? beforeTransition)
    {
        _to = to;
        _from = from;
        _predicate = predicate;
        _beforeTransition = beforeTransition;
    }

    public bool CanTransition()
        => _predicate(_from);

    public void BeforeTransition()
        => _beforeTransition?.Invoke(_to);

    public State To => _to;
}