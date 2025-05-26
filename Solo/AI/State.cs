using Microsoft.Xna.Framework;

namespace Solo.AI;

public abstract record State
{
    public State(GameObject owner)
    {
        this.Owner = owner;

        this.ElapsedMilliseconds = 0f;

        this.Id = IdGenerator<State>.Next();
    }

    protected virtual void OnEnter(Game game) { }

    protected virtual void OnExecute(Game game, GameTime gameTime) { }

    protected virtual void OnExit(Game game) { }

    public void Enter(Game game)
    {
        this.ElapsedMilliseconds = 0f;
        this.IsCompleted = false;
        this.OnEnter(game);
    }

    public void Execute(Game game, GameTime gameTime)
    {
        this.ElapsedMilliseconds += gameTime.ElapsedGameTime.TotalMilliseconds;
        OnExecute(game, gameTime);
    }

    public void Exit(Game game)
    {
        this.IsCompleted = true;
        this.OnExit(game);
    }

    #region Properties

    public readonly GameObject Owner;

    public double ElapsedMilliseconds { get; private set; }

    public bool IsCompleted { get; protected set; }

    public int Id { get; }

    #endregion Properties

    public override int GetHashCode()
        => HashCode.Combine(Owner.GetHashCode(), this.GetType(), this.Id);
}
