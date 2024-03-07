using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monoroids.Core.Assets;
using Monoroids.Core.Services;

namespace Monoroids.Core.Components;

public class SpriteRenderComponent : Component, IRenderable
{
    private TransformComponent _transform;
    
    public SpriteRenderComponent(GameObject owner) : base(owner)
    {
    }

    public void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Sprite.Texture, 
            position: _transform.World.Position,
            sourceRectangle: Sprite.Bounds, 
            color: Color.White, 
            rotation: _transform.World.Rotation,
            origin: Sprite.Center,
            scale: _transform.World.Scale,
            SpriteEffects.None,
            layerDepth: 0f);
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();
    }

    public Sprite Sprite;

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
}