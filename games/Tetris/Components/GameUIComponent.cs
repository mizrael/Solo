using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Components;
using Solo.Services;

namespace Tetris.Components;

public class GameUIComponent : Component, IRenderable
{
    private RenderService _renderService;

    private GameUIComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _renderService = GameServicesManager.Instance.GetService<RenderService>();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        _renderService.Graphics.GraphicsDevice.Clear(Color.Black);
      
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }

    public GameState GameState;
    public SpriteFont Font;
}