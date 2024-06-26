﻿using Microsoft.Xna.Framework;

namespace Solo.Components;

public class TransformComponent : Component
{
    private readonly Transform _local = Transform.Identity();
    private readonly Transform _world = Transform.Identity();

    private TransformComponent(GameObject owner) : base(owner)
    {
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        _world.Clone(_local);

        if (null != Owner.Parent && Owner.Parent.Components.TryGet<TransformComponent>(out var parentTransform))
            _world.Position = _local.Position + parentTransform.World.Position;
    }

    public void Reset()
    {
        Local.Reset();
        World.Reset();
    }

    public Transform Local => _local;
    public Transform World => _world;
}
