using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets;

namespace Solocaster.Components;

public class StaticFrameProvider : IFrameProvider
{
    private readonly Sprite _sprite;

    public StaticFrameProvider(Sprite sprite)
    {
        _sprite = sprite;
    }

    public Sprite? Sprite => _sprite;

    public Rectangle Bounds => _sprite.Bounds;

    public Texture2D GetTexture() => _sprite.Texture;
}
