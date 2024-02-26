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

    protected virtual void Init(Game game) { }

    protected virtual void UpdateCore() { }

    public virtual void Update(Game game)
    {
        if (!Owner.Enabled)
            return;

        if (!_initialized)
        {
            Init(game);
            _initialized = true;
        }
    }

    public GameObject Owner { get; }
    public bool Initialized => _initialized;
}
