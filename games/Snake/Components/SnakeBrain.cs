using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;

namespace Snake.Components;

public class SnakeBrain : Component
{
    public SnakeBrain(GameObject owner) : base(owner)
    {
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        base.UpdateCore(gameTime);
    }

    public Snake Snake { get; set; }
}