using Microsoft.Xna.Framework;

namespace Solo.AI;

public class StateMachine
{
    private readonly Dictionary<int, List<StateTransition>> _states;
    private State _currState;

    public StateMachine(IEnumerable<State> states)
    {
        _states = states.ToDictionary(s => s.Id, _ => new List<StateTransition>());
        _currState = states.First();
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
            _currState.Exit();

            validTransition.BeforeTransition();

            _currState = validTransition.To;
            _currState.Enter();
        }

        if (!_currState.IsCompleted)
            _currState.Execute(gameTime);
    }
}