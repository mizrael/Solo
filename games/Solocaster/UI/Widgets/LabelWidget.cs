using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solocaster.UI.Widgets;

public class LabelWidget : Widget
{
    private string _text = string.Empty;
    private string[]? _wrappedLines;
    private float _lastWrapWidth;

    public LabelWidget()
    {
    }

    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                _wrappedLines = null; // Invalidate cache
            }
        }
    }

    public SpriteFont? Font { get; set; }
    public Color TextColor { get; set; } = Color.White;
    public bool CenterHorizontally { get; set; } = false;
    public bool CenterVertically { get; set; } = false;
    public bool WordWrap { get; set; } = false;

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        if (Font == null || string.IsNullOrEmpty(Text))
            return;

        if (WordWrap)
        {
            RenderWrapped(spriteBatch);
        }
        else
        {
            RenderSingleLine(spriteBatch);
        }
    }

    private void RenderSingleLine(SpriteBatch spriteBatch)
    {
        var textSize = Font!.MeasureString(Text);
        var position = ScreenPosition;

        if (CenterHorizontally)
            position.X += (Size.X - textSize.X) / 2;

        if (CenterVertically)
            position.Y += (Size.Y - textSize.Y) / 2;

        spriteBatch.DrawString(Font, Text, position, TextColor);
    }

    private void RenderWrapped(SpriteBatch spriteBatch)
    {
        // Re-wrap if width changed or cache is invalid
        if (_wrappedLines == null || Math.Abs(_lastWrapWidth - Size.X) > 0.1f)
        {
            _wrappedLines = WrapText(Text, Size.X);
            _lastWrapWidth = Size.X;
        }

        var position = ScreenPosition;
        float lineHeight = Font!.LineSpacing;

        if (CenterVertically)
        {
            float totalHeight = _wrappedLines.Length * lineHeight;
            position.Y += (Size.Y - totalHeight) / 2;
        }

        foreach (var line in _wrappedLines)
        {
            var linePos = position;

            if (CenterHorizontally)
            {
                var lineWidth = Font.MeasureString(line).X;
                linePos.X += (Size.X - lineWidth) / 2;
            }

            spriteBatch.DrawString(Font, line, linePos, TextColor);
            position.Y += lineHeight;
        }
    }

    private string[] WrapText(string text, float maxWidth)
    {
        if (Font == null || string.IsNullOrEmpty(text))
            return Array.Empty<string>();

        var lines = new List<string>();
        var paragraphs = text.Split('\n');

        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrEmpty(paragraph))
            {
                lines.Add(string.Empty);
                continue;
            }

            var words = paragraph.Split(' ');
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                if (currentLine.Length == 0)
                {
                    // First word on line
                    if (Font.MeasureString(word).X > maxWidth)
                    {
                        // Word is too long, just add it anyway
                        lines.Add(word);
                    }
                    else
                    {
                        currentLine.Append(word);
                    }
                }
                else
                {
                    var testLine = currentLine + " " + word;
                    if (Font.MeasureString(testLine).X <= maxWidth)
                    {
                        currentLine.Append(' ');
                        currentLine.Append(word);
                    }
                    else
                    {
                        // Line would be too long, start new line
                        lines.Add(currentLine.ToString());
                        currentLine.Clear();
                        currentLine.Append(word);
                    }
                }
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());
        }

        return lines.ToArray();
    }
}
