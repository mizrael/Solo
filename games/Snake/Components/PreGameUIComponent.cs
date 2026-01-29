using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Snake.Scenes;
using Solo;
using Solo.Components;
using Solo.Services;

namespace Snake.Components;

public class PreGameUIComponent : Component, IRenderable
{
    private KeyboardState _prevKeyState = new();

    private PreGameUIComponent(GameObject owner) : base(owner)
    {
    }

    public void Render(SpriteBatch spriteBatch)
    {
        var graphicsDevice = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice;
        graphicsDevice.Clear(Color.Black);

        var scale = 2f;
        var size = Font.MeasureString(Text) * scale * .5f;

        var pos = new Vector2(
            graphicsDevice.Viewport.Width * .5f - size.X,
            graphicsDevice.Viewport.Height * .5f - size.Y);

        spriteBatch.DrawString(Font, Text, pos, Color.White,
                               0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        var text = "press Enter to start";
        pos.Y += size.Y * 2;
        scale = 1.5f;
        size = Font.MeasureString(text) * scale * .5f;
        pos.X = graphicsDevice.Viewport.Width * .5f - size.X;
        spriteBatch.DrawString(Font, text, pos, Color.White,
                               0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var canStart = _prevKeyState.IsKeyDown(Keys.Enter) && keyboardState.IsKeyUp(Keys.Enter);
        if (canStart)
            SceneManager.Instance.SetScene(SceneNames.Play);
        _prevKeyState = keyboardState;
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }

    public string Text;
    public SpriteFont Font;
}