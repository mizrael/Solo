using Microsoft.Xna.Framework;
using System;

namespace Monoroids.Core.Components;

public class LambdaComponent : Component
{
    private LambdaComponent(GameObject owner) : base(owner)
    {
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if(OnUpdate is not null)
            OnUpdate(this.Owner, gameTime);
    }

    public Action<GameObject, GameTime> OnUpdate;
}
