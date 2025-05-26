using Microsoft.Xna.Framework;

namespace Solo.AI;

public class StateMachine
{
    private readonly Dictionary<int, List<IStateTransition>> _states;
    private readonly Game _game;
    private State? _currState;

    public StateMachine(Game game)
    {
        _states = new ();
        _currState = null;
        _game = game;
    }

    public void AddTransition<TS1, TS2>(TS1 from, TS2 to, Predicate<TS1> predicate, Action<TS2>? beforeTransition)
        where TS1 : State
        where TS2 : State
    {
        if(!_states.ContainsKey(from.Id))
            _states.Add(from.Id, new List<IStateTransition>());
        _states[from.Id].Add(new StateTransition<TS1, TS2>(from, to, predicate, beforeTransition));
    }

    public void AddTransition<TS1, TS2>(TS1 from, TS2 to, Predicate<TS1> predicate)
        where TS1 : State
        where TS2 : State
        => this.AddTransition(from, to, predicate, null);

    public void SetState(State? state)
    {
        _currState?.Exit(_game);

        _currState = state;
        _currState?.Enter(_game);
    }

    public void Update(GameTime gameTime)
    {
        if (null == _currState)
            return;

        var transitions = _states[_currState.Id];
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