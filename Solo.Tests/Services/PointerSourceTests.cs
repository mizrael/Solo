using Microsoft.Xna.Framework.Input;
using Solo.Services;
using Xunit;

namespace Solo.Tests.Services;

public class PointerSourceTests : IDisposable
{
    public void Dispose() => PointerSource.Current = null;

    private sealed class StubPointerSource : IPointerSource
    {
        private readonly MouseState _state;
        public StubPointerSource(MouseState state) => _state = state;
        public MouseState GetState() => _state;
    }

    [Fact]
    public void GetState_WithNoCurrentSource_DoesNotThrow()
    {
        PointerSource.Current = null;
        var exception = Record.Exception(() => PointerSource.GetState());
        Assert.Null(exception);
    }

    [Fact]
    public void GetState_WithCurrentSource_ReturnsInjectedState()
    {
        var injected = new MouseState(
            410, 260, 0,
            ButtonState.Pressed, ButtonState.Released, ButtonState.Released,
            ButtonState.Released, ButtonState.Released);
        PointerSource.Current = new StubPointerSource(injected);

        var actual = PointerSource.GetState();

        Assert.Equal(410, actual.X);
        Assert.Equal(260, actual.Y);
        Assert.Equal(ButtonState.Pressed, actual.LeftButton);
    }

    [Fact]
    public void Current_WhenSetToNull_IsNull()
    {
        PointerSource.Current = new StubPointerSource(new MouseState(
            999, 999, 0,
            ButtonState.Pressed, ButtonState.Released, ButtonState.Released,
            ButtonState.Released, ButtonState.Released));

        PointerSource.Current = null;

        Assert.Null(PointerSource.Current);
    }
}
