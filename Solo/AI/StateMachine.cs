using Microsoft.Xna.Framework;

namespace Solo.AI;

public class StateMachine
{
    private readonly Dictionary<int, List<StateTransition>> _states;
    private readonly Game _game;
    private State _currState;

    public StateMachine(Game game, IEnumerable<State> states)
    {
        _states = states.ToDictionary(s => s.Id, _ => new List<StateTransition>());
        _currState = states.First();
        _game = game;
    }

    public void AddTransition(State from, State to, Predicate<State> predicate)
        => this.AddTransition(from, to, predicate, null);

    public void AddTransition(State from, State to, Predicate<State> predicate, Action<State>? beforeTransition)
        => _states[from.Id].Add(new StateTransition(from, to, predicate, beforeTransition));

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