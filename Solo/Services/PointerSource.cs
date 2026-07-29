using Microsoft.Xna.Framework.Input;

namespace Solo.Services;

/// <summary>
/// Supplies pointer state to the UI layer. Implement this to feed the UI a
/// synthetic cursor instead of the physical mouse.
/// </summary>
public interface IPointerSource
{
    /// <summary>Returns the pointer state for the current frame, in raw screen pixels.</summary>
    MouseState GetState();
}

/// <summary>
/// Global registration point for the active <see cref="IPointerSource"/>. When no source is
/// registered the physical mouse is used, which is the behaviour every shipping game relies on.
/// </summary>
public static class PointerSource
{
    /// <summary>The active pointer source, or <see langword="null"/> to use the physical mouse.</summary>
    public static IPointerSource? Current { get; set; }

    /// <summary>Reads the current pointer state from <see cref="Current"/>, or the physical mouse.</summary>
    public static MouseState GetState() => Current?.GetState() ?? Mouse.GetState();
}
