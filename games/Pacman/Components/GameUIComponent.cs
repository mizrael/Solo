using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solo.Utils;

namespace Pacman.Components;

public sealed class GameUIComponent : Component, IRenderable
{
    private RenderService _renderService;

    private GameUIComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _renderService = GameServicesManager.Instance.GetRequired<RenderService>();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        var scale = 1f;
        var pos = new Vector2(10, 10);
        var text = $"Score: {GameState.Score}";
        spriteBatch.DrawString(Font, text, pos, Color.White,
                               0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
    
    public GameState GameState;

    public SpriteFont Font;
    public Vector2 TileSize = new(16, 16);
}
