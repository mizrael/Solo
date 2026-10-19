using Microsoft.Xna.Framework;
using Solo.UI.Layout;
using Xunit;

namespace Solo.Tests.UI.Layout;

public sealed class WrappedTextTests
{
    private readonly FakeTextMeasurer _measurer = new();
    private readonly WrappedText _wrapped = new();

    [Fact]
    public void Update_WrapsAtSpaces()
    {
        _wrapped.Update("aaa bbb ccc", 70, 0, _measurer);

        Assert.Equal(["aaa bbb", "ccc"], _wrapped.Lines);
        Assert.Equal(new Vector2(70, 40), _wrapped.Size);
    }

    [Fact]
    public void Update_CollapsesRepeatedSpaces()
    {
        _wrapped.Update("  aa   bb  ", 0, 0, _measurer);

        Assert.Equal(["aa bb"], _wrapped.Lines);
    }

    [Fact]
    public void Update_BreaksWordsLongerThanTheWidth()
    {
        _wrapped.Update("abcdefghij", 40, 0, _measurer);

        Assert.Equal(["abcd", "efgh", "ij"], _wrapped.Lines);
        Assert.Equal(new Vector2(40, 60), _wrapped.Size);
    }

    [Fact]
    public void Update_LongWordStartsOnItsOwnLineAndTailJoinsNextWord()
    {
        _wrapped.Update("ab abcdef g", 40, 0, _measurer);

        Assert.Equal(["ab", "abcd", "ef g"], _wrapped.Lines);
    }

    [Fact]
    public void Update_KeepsNewlinesAndEmptyLines()
    {
        _wrapped.Update("a\r\n\nb", 100, 0, _measurer);

        Assert.Equal(["a", "", "b"], _wrapped.Lines);
        Assert.Equal(new Vector2(10, 60), _wrapped.Size);
    }

    [Fact]
    public void Update_WithoutWidth_DoesNotWrap()
    {
        _wrapped.Update("aaa bbb ccc", 0, 0, _measurer);

        Assert.Equal(["aaa bbb ccc"], _wrapped.Lines);
        Assert.Equal(new Vector2(110, 20), _wrapped.Size);
    }

    [Fact]
    public void Update_EmptyText_HasNoLines()
    {
        _wrapped.Update(string.Empty, 100, 0, _measurer);

        Assert.Empty(_wrapped.Lines);
        Assert.Equal(Vector2.Zero, _wrapped.Size);
    }

    [Fact]
    public void Update_OverMaxLines_TruncatesWithEllipsis()
    {
        _wrapped.Update("aaa bbb ccc ddd", 70, 1, _measurer);

        Assert.Equal(["aaa..."], _wrapped.Lines);
        Assert.Equal(new Vector2(60, 20), _wrapped.Size);
    }

    [Fact]
    public void Update_WithinMaxLines_DoesNotTruncate()
    {
        _wrapped.Update("aaa bbb ccc", 70, 2, _measurer);

        Assert.Equal(["aaa bbb", "ccc"], _wrapped.Lines);
    }

    [Fact]
    public void Update_SameInputs_DoesNotRewrap()
    {
        _wrapped.Update("aaa bbb", 40, 0, _measurer);
        int measured = _measurer.MeasureCount;

        bool rewrapped = _wrapped.Update("aaa bbb", 40, 0, _measurer);

        Assert.False(rewrapped);
        Assert.Equal(measured, _measurer.MeasureCount);
    }

    [Fact]
    public void Update_WidthChanged_Rewraps()
    {
        _wrapped.Update("aaa bbb", 40, 0, _measurer);

        bool rewrapped = _wrapped.Update("aaa bbb", 100, 0, _measurer);

        Assert.True(rewrapped);
        Assert.Equal(["aaa bbb"], _wrapped.Lines);
    }
}
