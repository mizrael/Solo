using System;
using Microsoft.Xna.Framework;

namespace Monoroids.Core.Services;

public abstract class Scene
{
    protected Game Game { get; }

    protected Scene(Game game)
    {
        this.Game = game ?? throw new ArgumentNullException(nameof(game));
    }

    public void Step(GameTime gameTime)
    {
        if (null != Root)
            Root.Update(gameTime);
        this.Update();
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
    protected virtual void Update() { }

    public GameObject Root { get; private set; }
}
