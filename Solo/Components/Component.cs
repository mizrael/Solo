using Microsoft.Xna.Framework;
using System;

namespace Monoroids.Core.Components;

public abstract class Component
{
    private bool _initialized = false;

    protected Component(GameObject owner)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public void Init()
    {
        if (_initialized)
            return;

        InitCore();
        _initialized = true;
    }

    protected virtual void InitCore() { }

    public void Update(GameTime gameTime)
    {
        if (!Owner.Enabled)
            return;

        Init();

        UpdateCore(gameTime);
    }
    protected virtual void UpdateCore(GameTime gameTime) { }

    public GameObject Owner { get; }
    public bool Initialized => _initialized;
}
