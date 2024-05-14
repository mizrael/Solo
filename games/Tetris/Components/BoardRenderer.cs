using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solo.Utils;

namespace Tetris.Components;

public class BoardRenderer : Component, IRenderable
{
    private Texture2D _texture;

    public BoardRenderer(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        var renderService = GameServicesManager.Instance.GetService<RenderService>();

        _texture = renderService.CreateTexture(Constants.TileTextureWidth, Constants.TileTextureHeight, Constants.TileTextureData);

        base.InitCore();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        var rect = new Rectangle(0, 0, Constants.TileTextureWidth, Constants.TileTextureHeight);
        
        for (var y = 0; y < Board.Height; y++)
        {
            for (var x = 0; x < Board.Width; x++)
            {
                var tile = Board.GetTileAt(x, y);
                var color = Color.White; //tile.Color ?? Color.Gray;

                var position = new Vector2(x * TileSize.X, y * TileSize.Y);
                
                spriteBatch.Draw(_texture, position, rect, color, 0f, Vector2.Zero, TileSize, SpriteEffects.None, LayerIndex);
            }
        }
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
    public Vector2 TileSize;
    public Board Board;
}
