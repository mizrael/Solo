using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets;

namespace Solocaster.Components;

public interface IFrameProvider
{
    public Sprite? Sprite { get; }

    public Rectangle Bounds { get; }
}
