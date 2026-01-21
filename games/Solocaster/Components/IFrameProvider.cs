using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solocaster.Components;

public interface IFrameProvider
{
    Rectangle GetCurrentBounds();
    Texture2D? GetTexture();
}
