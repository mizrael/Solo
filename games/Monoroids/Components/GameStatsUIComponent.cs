using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using System;

namespace Monoroids.Components;

public class GameStatsUIComponent : Component, IRenderable
{
    private int _score = 0;
    private int _maxScore = 0;
    private string _text;
    private readonly Vector2 _position = new (25, 25);

    private GameStatsUIComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        var hiScore = Math.Max(_score, _maxScore);
        _text = $"Score: {_score}\nHi Score: {hiScore}";
    }

    public void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(this.Font, _text, _position, Color.White);
    }

    public void IncreaseScore(int points)
    {
        _score += points;
        UpdateText();
    }

    public void ResetScore()
    {
        _maxScore = Math.Max(_score, _maxScore);
        _score = 0;
        UpdateText();
    }

    public SpriteFont Font;

    public int LayerIndex { get; set; }

    public bool Hidden { get; set; }
}
