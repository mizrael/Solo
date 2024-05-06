using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solo.Utils;

namespace Snake.Components;

public class SnakeRenderer : Component, IRenderable
{
    private Texture2D _texture;

    public SnakeRenderer(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        var renderService = GameServicesManager.Instance.GetService<RenderService>();
        
        _texture = renderService.CreateTexture(1, 1, Color.White);

        base.InitCore();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        if(Hidden)
            return;

        var color = Color.Green;

        var segment = Snake.Head;
        while (segment != null)
        {
            var pos = segment.Tile.ToVector2() * TileSize;
            spriteBatch.Draw(_texture, pos, null, color, 0f, Vector2.Zero, TileSize, SpriteEffects.None, LayerIndex);

            segment = segment.Next;
        }
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
    public Vector2 TileSize { get; set; } = new Vector2(16, 16);

    public Snake Snake { get; set; }
}
