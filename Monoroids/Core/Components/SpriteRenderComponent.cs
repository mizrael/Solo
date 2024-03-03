using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monoroids.Core.Assets;
using Monoroids.Core.Services;

namespace Monoroids.Core.Components;

public class SpriteRenderComponent : Component, IRenderable
{
    private TransformComponent _transform;
    private readonly Sprite _sprite;
    
    public SpriteRenderComponent(GameObject owner, Sprite sprite) : base(owner)
    {
        _sprite = sprite;
    }

    public void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_sprite.Texture, 
            position: _transform.World.Position,
            sourceRectangle: _sprite.Bounds, 
            color: Color.White, 
            rotation: _transform.World.Rotation,
            origin: Vector2.Zero,
            scale: _transform.World.Scale,
            SpriteEffects.None,
            layerDepth: 0f);
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
}