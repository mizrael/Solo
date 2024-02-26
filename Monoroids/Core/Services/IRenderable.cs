using Microsoft.Xna.Framework.Graphics;

namespace Monoroids.Core.Services;

public interface IRenderable
{
    void Render(SpriteBatch spriteBatch);

    int LayerIndex { get; set; }

    bool Hidden { get; set; }
}