using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monoroids.Core;
using Monoroids.Core.Components;
using Monoroids.Core.Services;
using Monoroids.Core.GUI;
using System.Linq;

namespace Monoroids.GameStuff.Components;

public class ShipSelectionUIComponent : Component, IRenderable
{
    private RenderService? _renderService;

    private static readonly GUILine[] _textLines = new[]{
        "Select your ship",
        "Press ENTER to start"
    }.Select(line => new GUILine(line)).ToArray();

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
        var prevHeight = 0f;
        var lineSpacing = 30f;
        Vector2 halfScreen = new(
                            _renderService!.Graphics.PreferredBackBufferWidth * .5f,
                            _renderService!.Graphics.PreferredBackBufferHeight * .5f),
            pos = Vector2.Zero;

        foreach(var line in _textLines)
        {
            pos = new Vector2(
                            halfScreen.X - line.Size.X,
                            line.Size.Y + prevHeight + lineSpacing);
            line.Render(spriteBatch, pos, scale);

            prevHeight = pos.Y;
        }   

        var nameLine = new GUILine(this.SelectedShip.Name, Font);
        pos = new Vector2(halfScreen.X - nameLine.Size.X,
                          _renderService!.Graphics.PreferredBackBufferHeight - nameLine.Size.Y * 4f);
        nameLine.Render(spriteBatch, pos, scale);
    }

    private SpriteFont? _font;
    public SpriteFont? Font
    {
        get => _font;
        set{
            _font = value;
            for(int i = 0; i < _textLines.Length; i++)
                _textLines[i].Font = value;
        }
    }

    public ShipTemplate SelectedShip;

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
}
