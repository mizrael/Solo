using Microsoft.Xna.Framework;

namespace Solo.Services;

public sealed class SceneObjectsGraph : IGameService
{
    public GameObject Root { get; } = new();

    public void Update(GameTime gameTime)
    {
        Root.Update(gameTime);
    }
}
