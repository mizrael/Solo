using Microsoft.Xna.Framework.Input;
using Solo.UI;
using Xunit;

namespace Solo.Tests.UI;

/// <summary>
/// Proves that <see cref="UIService.ScaleMouseState"/> applies the UI scale exactly
/// once. UIService.Update cannot be exercised in a unit test because it requires a
/// live GraphicsDevice via GraphicsDeviceManagerAccessor. ScaleMouseState is the
/// lowest testable level: widened to internal so the test assembly can reach it.
///
/// A viewport height of 540 yields UIScale = 540/1080 = 0.5, a value that makes
/// single vs double scaling distinguishable: raw (200, 150) scaled once = (400, 300),
/// scaled twice = (800, 600).
/// </summary>
public sealed class UIServiceScalingTests : IDisposable
{
    public UIServiceScalingTests() => UITheme.UpdateUIScale(540);

    public void Dispose() => UITheme.UpdateUIScale(1080);

    [Fact]
    public void ScaleMouseState_WithScaleOneHalf_DividesCoordinatesByScale()
    {
        var raw = new MouseState(
            200, 150, 0,
            ButtonState.Released, ButtonState.Released, ButtonState.Released,
            ButtonState.Released, ButtonState.Released);

        var scaled = UIService.ScaleMouseState(raw);

        // scale = 0.5: each coordinate is divided by 0.5, i.e. doubled.
        // If scaling were applied twice the results would be 800 and 600.
        Assert.Equal(400, scaled.X);
        Assert.Equal(300, scaled.Y);
    }

    [Fact]
    public void ScaleMouseState_WithScaleOneHalf_PreservesButtonState()
    {
        var raw = new MouseState(
            0, 0, 0,
            ButtonState.Pressed, ButtonState.Released, ButtonState.Released,
            ButtonState.Released, ButtonState.Released);

        var scaled = UIService.ScaleMouseState(raw);

        Assert.Equal(ButtonState.Pressed, scaled.LeftButton);
    }

    [Fact]
    public void ScaleMouseState_WithScaleEqualToOne_ReturnsUnchangedState()
    {
        UITheme.UpdateUIScale(1080);  // ensure scale = 1 for this case
        var raw = new MouseState(
            300, 200, 0,
            ButtonState.Released, ButtonState.Released, ButtonState.Released,
            ButtonState.Released, ButtonState.Released);

        var scaled = UIService.ScaleMouseState(raw);

        Assert.Equal(300, scaled.X);
        Assert.Equal(200, scaled.Y);
    }
}
