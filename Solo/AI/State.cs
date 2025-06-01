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

    protected virtual void OnEnter() { }

    protected virtual void OnExecute(GameTime gameTime) { }

    protected virtual void OnExit() { }

    public void Enter()
    {
        this.ElapsedMilliseconds = 0f;
        this.IsCompleted = false;
        this.OnEnter();
    }

    public void Execute(GameTime gameTime)
    {
        this.ElapsedMilliseconds += gameTime.ElapsedGameTime.TotalMilliseconds;
        OnExecute(gameTime);
    }

    public void Exit()
    {
        this.IsCompleted = true;
        this.OnExit();
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
