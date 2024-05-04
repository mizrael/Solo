using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Components;
using Solo.Services;
using Monoroids.GameStuff.Scenes;

namespace Monoroids.GameStuff.Components;

public class PreGameUIComponent : Component, IRenderable
{
    private RenderService _renderService;

    private PreGameUIComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _renderService = GameServicesManager.Instance.GetService<RenderService>();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        var scale = 2f;
        var size = this.Font.MeasureString(this.Text) * scale * .5f;

        var pos = new Vector2(
            (float)_renderService.Graphics.PreferredBackBufferWidth * .5f - size.X,
            (float)_renderService.Graphics.PreferredBackBufferHeight * .5f - size.Y);
    
        spriteBatch.DrawString(this.Font, this.Text, pos, Color.White, 
                               0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        var text = "press Enter to start";
        pos.Y += size.Y * 2;
        scale = 1.5f;
        size = this.Font.MeasureString(text) * scale * .5f;
        pos.X = (float)_renderService.Graphics.PreferredBackBufferWidth * .5f - size.X;
        spriteBatch.DrawString(this.Font, text, pos, Color.White,
                               0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var canStart = keyboardState.IsKeyDown(Keys.Enter);
        if (canStart)
            GameServicesManager.Instance.GetService<SceneManager>().SetCurrentScene(SceneNames.ShipSelection);
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }

    public string Text = "Blazeroids!";
    public SpriteFont Font;
}