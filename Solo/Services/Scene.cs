using Microsoft.Xna.Framework;

namespace Solo.Services;

public abstract class Scene
{
    public Game Game { get; }

    protected Scene(Game game)
    {
        this.Game = game ?? throw new ArgumentNullException(nameof(game));
    }

    public void Step(GameTime gameTime)
    {
        Root?.Update(gameTime);
        this.Update(gameTime);
    }

    public void Enter()
    {
        this.Root = new GameObject();
        this.EnterCore();
    }

    protected virtual void EnterCore() { }

    public void Exit()
    {
        this.Root = null;
        this.ExitCore();
    }

    protected virtual void ExitCore() { }
    protected virtual void Update(GameTime gameTime) { }

    public GameObject Root { get; private set; }
}
