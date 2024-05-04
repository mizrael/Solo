using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services;

public interface IRenderable
{
    void Render(SpriteBatch spriteBatch);

    int LayerIndex { get; set; }

    bool Hidden { get; set; }
}