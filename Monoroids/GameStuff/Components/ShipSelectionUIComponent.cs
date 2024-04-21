using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monoroids.Core;
using Monoroids.Core.Components;
using Monoroids.Core.Services;

namespace Monoroids.GameStuff.Components;

public class ShipSelectionUIComponent : Component, IRenderable
{
    private RenderService? _renderService;

    private static string[] _text = [
        "Select your ship",
        "Press ENTER to start"
    ];

    private ShipSelectionUIComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _renderService = GameServicesManager.Instance.GetService<RenderService>();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        var scale = 2f;

        var prevLineHeight = 0f;
        foreach(var line in _text)
        {
            var lineSize = Font!.MeasureString(line) * scale * .5f;

            var pos = new Vector2(_renderService!.Graphics.PreferredBackBufferWidth * .5f - lineSize.X,
                                  (lineSize.Y + prevLineHeight) * scale);

            spriteBatch.DrawString(Font, line, pos, Color.White,
                                   0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            prevLineHeight = lineSize.Y;
        }   
    }

    public SpriteFont? Font;

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
}