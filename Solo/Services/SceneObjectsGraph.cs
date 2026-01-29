using Microsoft.Xna.Framework;

namespace Solo.Services;

public sealed class SceneObjectsGraph : IGameService
{
    public GameObject Root { get; private set; } = new();

    public void Reset()
    {
        Root = new();
    }

    public void Update(GameTime gameTime)
    {
        Root.Update(gameTime);
    }
}
