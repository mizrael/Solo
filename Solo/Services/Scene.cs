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
        if (null != Root)
            Root.Update(gameTime);
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

    public GameObject? FindFirst(Func<GameObject, bool> predicate)
    {
        if (Root == null)
            return null;

        var compiledPredicate = predicate;

        var q = new Queue<GameObject>();
        q.Enqueue(Root);
        while (q.Any())
        {
            var obj = q.Dequeue();
            if (compiledPredicate(obj))
                return obj;
            foreach (var child in obj.Children)
                q.Enqueue(child);
        }
        return null;
    }
}
