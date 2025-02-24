﻿using Solo;
using Solo.Components;
using Solo.Services;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Monoroids.Components;

public class PlayerStatsUIComponent : Component, IRenderable
{
    private const int _maxWidth = 200;
    private const int _maxHeight = 20;
    private const int _bottomOffset = 20;
    private const int _rightOffset = 20;

    private Color _shieldColor = new (68, 68, 255);
    private RenderService _renderService;
    private Texture2D _texture;

    private PlayerStatsUIComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _renderService = GameServicesManager.Instance.GetRequired<RenderService>();

        _texture = Texture2DUtils.Generate(_renderService.Graphics.GraphicsDevice, 1, 1, Color.White);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        RenderHealth(spriteBatch);
        RenderShield(spriteBatch);
    }

    private void RenderShield(SpriteBatch spriteBatch)
    {
        float ratio = (float)this.PlayerBrain.Stats.ShieldPower / this.PlayerBrain.Stats.ShieldMaxPower;
        int width = (int)(ratio * _maxWidth);
        
        int x = _renderService.Graphics.GraphicsDevice.Viewport.Width - width - _rightOffset;
        int y = _renderService.Graphics.GraphicsDevice.Viewport.Height - _maxHeight - _bottomOffset - _maxHeight - 5;

        spriteBatch.Draw(_texture, new Rectangle(x, y, width, _maxHeight), _shieldColor);
    }

    private void RenderHealth(SpriteBatch spriteBatch)
    {
        float ratio = (float)this.PlayerBrain.Stats.Health / this.PlayerBrain.Stats.MaxHealth;
        int width = (int)(ratio * _maxWidth);

        int x = _renderService.Graphics.GraphicsDevice.Viewport.Width - width - _rightOffset;
        int y = _renderService.Graphics.GraphicsDevice.Viewport.Height - _maxHeight - _bottomOffset;

        var color = ratio > .5 ? Color.Green : Color.Red;
        spriteBatch.Draw(_texture, new Rectangle(x, y, width, _maxHeight), color);
    }

    public PlayerBrain PlayerBrain { get; set; }
    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
}
