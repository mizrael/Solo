using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.GUI;

public record struct GUILine
{
    private string _text;
    private Vector2 _size;
    private SpriteFont? _font;

    public GUILine(string text, SpriteFont? font = null)
    {
        _text = text;
        _font = font;
        Measure();
    }

    public void Render(SpriteBatch spriteBatch, Vector2 pos, float scale = 1f)
    {
        spriteBatch.DrawString(_font, _text, pos, Color.White,
                               0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    public string Text 
    {
        get => _text; 
        set 
        {
            _text = value; 
            Measure(); 
        } 
    }

    public SpriteFont? Font
    { 
        get => _font; 
        set
        {
            _font = value;
            Measure();
        } 
    }

    public Vector2 Size => _size;

    private void Measure(){
        if(Font is not null)
            _size = Font.MeasureString(_text);
    }
}