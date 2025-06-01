using Microsoft.Xna.Framework;

namespace Solo.AI;

public class StateMachine
{
    private readonly Dictionary<int, List<IStateTransition>> _transitionsByState;
    private readonly Game _game;
    private readonly State _startState;
    private State? _currState;

    public StateMachine(Game game, State startState)
    {
        _transitionsByState = new ();
        _startState = startState;
        _game = game;
    }

    public void AddTransition<TS1, TS2>(TS1 from, TS2 to, Predicate<TS1> predicate, Action<TS2>? beforeTransition)
        where TS1 : State
        where TS2 : State
    {
        if(!_transitionsByState.ContainsKey(from.Id))
            _transitionsByState.Add(from.Id, new List<IStateTransition>());
        _transitionsByState[from.Id].Add(new StateTransition<TS1, TS2>(from, to, predicate, beforeTransition));
    }

    public void AddTransition<TS1, TS2>(TS1 from, TS2 to, Predicate<TS1> predicate)
        where TS1 : State
        where TS2 : State
        => this.AddTransition(from, to, predicate, null);

    public void Reset()
    {
        _currState?.Exit(_game);

        _currState = _startState;
        _currState?.Enter(_game);
    }

    public void Update(GameTime gameTime)
    {
        if (null == _currState)
        {
            _currState = _startState;
            _currState.Enter(_game);
            return;
        }

        var transitions = _transitionsByState[_currState.Id];
        var validTransition = transitions.FirstOrDefault(t => t.CanTransition());
        if (null != validTransition)
        {
            _currState.Exit(_game);

            validTransition.BeforeTransition();

            _currState = validTransition.To;
            _currState.Enter(_game);
        }

        if (!_currState.IsCompleted)
            _currState.Execute(_game, gameTime);
    }
}