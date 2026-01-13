using Microsoft.Xna.Framework;
using Solo;
using Solo.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solocaster.Services;

public class EntityManager : IGameService
{
    private readonly List<GameObject> _entities = new();

    public void Initialize()
    {
    }

    public void Step(GameTime gameTime)
    {
        // EntityManager doesn't need per-frame updates
        // Entities update themselves via Component.UpdateCore()
    }

    public void Register(GameObject entity)
    {
        if (!_entities.Contains(entity))
            _entities.Add(entity);
    }

    public void Unregister(GameObject entity)
    {
        _entities.Remove(entity);
    }

    public IEnumerable<GameObject> GetVisibleEntities(Func<GameObject, bool> isVisible)
    {
        return _entities.Where(e => e.Enabled && isVisible(e));
    }

    public void Clear()
    {
        _entities.Clear();
    }
}
