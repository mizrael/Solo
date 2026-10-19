using Microsoft.Xna.Framework;
using System.Text;

namespace Solo.UI.Layout;

/// <summary>
/// Word-wraps text into cached lines. Re-wraps only when the text, width, line limit or
/// measurer changes, so it is safe to call <see cref="Update"/> every layout pass.
/// </summary>
public sealed class WrappedText
{
    public const string Ellipsis = "...";

    private readonly List<string> _lines = new();
    private readonly List<float> _lineWidths = new();
    private readonly StringBuilder _line = new();
    private string? _text;
    private float _maxWidth;
    private int _maxLines;
    private ITextMeasurer? _measurer;

    public IReadOnlyList<string> Lines => _lines;
    public IReadOnlyList<float> LineWidths => _lineWidths;
    public Vector2 Size { get; private set; }

    /// <param name="maxWidth">Wrap width; zero or less means no wrapping.</param>
    /// <param name="maxLines">Line limit; zero or less means unlimited. Truncated text ends with <see cref="Ellipsis"/>.</param>
    /// <returns>True when the text was re-wrapped.</returns>
    public bool Update(string text, float maxWidth, int maxLines, ITextMeasurer measurer)
    {
        ArgumentNullException.ThrowIfNull(measurer);
        text ??= string.Empty;

        if (_measurer == measurer && _maxWidth == maxWidth && _maxLines == maxLines && string.Equals(_text, text, StringComparison.Ordinal))
            return false;

        _text = text;
        _maxWidth = maxWidth;
        _maxLines = maxLines;
        _measurer = measurer;

        Wrap(text, maxWidth, measurer);
        if (maxLines > 0 && _lines.Count > maxLines)
            Truncate(maxWidth, maxLines, measurer);

        float width = 0;
        for (int i = 0; i < _lineWidths.Count; i++)
            width = Math.Max(width, _lineWidths[i]);
        Size = new Vector2(width, _lines.Count * measurer.LineHeight);
        return true;
    }

    private void Wrap(string text, float maxWidth, ITextMeasurer measurer)
    {
        _lines.Clear();
        _lineWidths.Clear();
        if (text.Length == 0)
            return;

        int start = 0;
        while (true)
        {
            int newline = text.IndexOf('\n', start);
            int end = newline < 0 ? text.Length : newline;
            WrapParagraph(text, start, end, maxWidth, measurer);
            if (newline < 0)
                break;
            start = newline + 1;
        }
    }

    private void WrapParagraph(string text, int start, int end, float maxWidth, ITextMeasurer measurer)
    {
        _line.Clear();
        int i = start;
        while (i < end)
        {
            if (IsSpace(text[i]))
            {
                i++;
                continue;
            }

            int wordEnd = i;
            while (wordEnd < end && !IsSpace(text[wordEnd]))
                wordEnd++;

            AppendWord(text, i, wordEnd, maxWidth, measurer);
            i = wordEnd;
        }

        CommitLine(measurer);
    }

    private void AppendWord(string text, int start, int end, float maxWidth, ITextMeasurer measurer)
    {
        if (_line.Length > 0)
        {
            int lengthBefore = _line.Length;
            _line.Append(' ').Append(text, start, end - start);
            if (Fits(maxWidth, measurer))
                return;

            _line.Length = lengthBefore;
            CommitLine(measurer);
        }

        _line.Append(text, start, end - start);
        if (Fits(maxWidth, measurer))
            return;

        BreakWord(text, start, end, maxWidth, measurer);
    }

    private void BreakWord(string text, int start, int end, float maxWidth, ITextMeasurer measurer)
    {
        _line.Clear();
        for (int i = start; i < end; i++)
        {
            _line.Append(text[i]);
            if (_line.Length > 1 && !Fits(maxWidth, measurer))
            {
                _line.Length--;
                CommitLine(measurer);
                _line.Append(text[i]);
            }
        }
    }

    private void Truncate(float maxWidth, int maxLines, ITextMeasurer measurer)
    {
        _lines.RemoveRange(maxLines, _lines.Count - maxLines);
        _lineWidths.RemoveRange(maxLines, _lineWidths.Count - maxLines);

        int last = maxLines - 1;
        _line.Clear().Append(_lines[last]);
        TrimEndSpaces();
        while (true)
        {
            _line.Append(Ellipsis);
            if (_line.Length == Ellipsis.Length || Fits(maxWidth, measurer))
                break;
            _line.Length -= Ellipsis.Length + 1;
            TrimEndSpaces();
        }

        _lines[last] = _line.ToString();
        _lineWidths[last] = measurer.MeasureWidth(_line);
        _line.Clear();
    }

    private void TrimEndSpaces()
    {
        while (_line.Length > 0 && _line[_line.Length - 1] == ' ')
            _line.Length--;
    }

    private void CommitLine(ITextMeasurer measurer)
    {
        _lines.Add(_line.ToString());
        _lineWidths.Add(measurer.MeasureWidth(_line));
        _line.Clear();
    }

    private bool Fits(float maxWidth, ITextMeasurer measurer) =>
        maxWidth <= 0 || measurer.MeasureWidth(_line) <= maxWidth;

    private static bool IsSpace(char c) => c == ' ' || c == '\r' || c == '\t';
}
