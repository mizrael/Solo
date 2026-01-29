using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solo.GUI;
using System.Linq;
using Solo.Assets;

namespace Monoroids.Components;

public class ShipSelectionUIComponent : Component, IRenderable
{
    private ShipTemplate _selectedShipTemplate;
    private Sprite _selectedShipSprite;

    private static readonly GUILine[] _textLines = new[]{
        "Select your ship",
        "Press ENTER to start"
    }.Select(line => new GUILine(line)).ToArray();

    private ShipSelectionUIComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
    }

    public void Render(SpriteBatch spriteBatch)
    {
        var viewport = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport;

        var scale = 2f;
        var lineSpacing = 30f;
        Vector2 halfScreen = new(
                            viewport.Width * .5f,
                            viewport.Height * .5f),
            pos = Vector2.Zero;

        foreach (var line in _textLines)
        {
            pos.X = halfScreen.X - line.Size.X;
            pos.Y += line.Size.Y + lineSpacing;
            line.Render(spriteBatch, pos, scale);
        }

        if (_selectedShipSprite == null)
            return;

        var startX = (float)viewport.Width * .125f;

        var tmpLine = new GUILine(_selectedShipTemplate.Name, Font);
        pos = new Vector2(startX,
                          viewport.Height * .35f);
        tmpLine.Render(spriteBatch, pos, scale);

        scale = 1f;

        tmpLine.Text = $"health: {_selectedShipTemplate.Stats.MaxHealth}";
        pos.Y += tmpLine.Size.Y + lineSpacing;
        tmpLine.Render(spriteBatch, pos, scale);

        lineSpacing = 10f;

        tmpLine.Text = $"health recharge rate: {_selectedShipTemplate.Stats.HealthRegenRate}s";
        pos.Y += tmpLine.Size.Y + lineSpacing;
        tmpLine.Render(spriteBatch, pos, scale);

        tmpLine.Text = $"shields: {_selectedShipTemplate.Stats.ShieldMaxPower}";
        pos.Y += tmpLine.Size.Y + lineSpacing;
        tmpLine.Render(spriteBatch, pos, scale);

        tmpLine.Text = $"shields recharge rate: {_selectedShipTemplate.Stats.ShieldRechargeRate}s";
        pos.Y += tmpLine.Size.Y + lineSpacing;
        tmpLine.Render(spriteBatch, pos, scale);

        tmpLine.Text = $"engine power: {_selectedShipTemplate.Stats.EnginePower}";
        pos.Y += tmpLine.Size.Y + lineSpacing;
        tmpLine.Render(spriteBatch, pos, scale);

        tmpLine.Text = $"rotation speed: {_selectedShipTemplate.Stats.RotationSpeed}";
        pos.Y += tmpLine.Size.Y + lineSpacing;
        tmpLine.Render(spriteBatch, pos, scale);
    }

    private SpriteFont? _font;
    public SpriteFont? Font
    {
        get => _font;
        set
        {
            _font = value;
            for (int i = 0; i < _textLines.Length; i++)
                _textLines[i].Font = value;
        }
    }

    public void SetSelectedShip(GameObject ship, ShipTemplate template)
    {
        _selectedShipTemplate = template;
        _selectedShipSprite = ship.Components.Get<SpriteRenderComponent>().Sprite;
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
}
