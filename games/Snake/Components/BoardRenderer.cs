using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solo.Utils;

namespace Snake.Components;

public class BoardRenderer : Component, IRenderable
{
    private Texture2D _texture;

    public BoardRenderer(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();
        
        _texture = renderService.CreateTexture(1, 1, Color.White);

        base.InitCore();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        for (var y = 0; y < Board.Height; y++)
        {
            for (var x = 0; x < Board.Width; x++)
            {
                var tile = Board.GetTileAt(x, y);

                var color = tile switch
                {
                    TileType.Empty => Color.Black,
                    TileType.Wall => Color.White,
                    TileType.Food => Color.Red,
                    _ => Color.Black
                };

                var position = new Vector2(x * TileSize.X, y * TileSize.Y);
                spriteBatch.Draw(_texture, position, null, color, 0f, Vector2.Zero, TileSize, SpriteEffects.None, LayerIndex);
            }
        }
    }

    public Vector2 TileSize { get; set; } = new Vector2(16, 16);
    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
    public Board Board { get; set; }
}
