using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monoroids.Core.Services;

namespace Monoroids.Core.Components;

public class SpriteRenderComponent : Component, IRenderable
{
    private TransformComponent _transform;
    private readonly Texture2D _texture;
    
    public SpriteRenderComponent(GameObject owner, Texture2D texture) : base(owner)
    {
        _texture = texture;
    }

    public void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_texture, 
            position: _transform.World.Position,
            sourceRectangle: null, 
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