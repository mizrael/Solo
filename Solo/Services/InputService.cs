using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Solo.Services;

/// <summary>
/// Manages keyboard input with configurable action-to-key bindings.
/// Can be instantiated per-Scene, since each Scene might have its own bindings.
/// </summary>
public class InputService : IGameService
{
    private readonly Dictionary<string, Keys> _defaultBindings = new();
    private readonly Dictionary<string, Keys> _bindings = new();
    private KeyboardState _currentState;
    private KeyboardState _previousState;

    public InputService(
        IReadOnlyDictionary<string, Keys> defaultBindings,
        IReadOnlyDictionary<string, Keys>? userOverrides = null)
    {
        foreach (var kvp in defaultBindings)
        {
            _defaultBindings[kvp.Key] = kvp.Value;
            _bindings[kvp.Key] = kvp.Value;
        }

        if (userOverrides != null)
        {
            foreach (var kvp in userOverrides)
            {
                if (_bindings.ContainsKey(kvp.Key))
                    _bindings[kvp.Key] = kvp.Value;
            }
        }

        // Initialize both states to current keyboard state to prevent
        // keys held during scene transition from appearing as "just pressed"
        _currentState = Keyboard.GetState();
        _previousState = _currentState;
    }

    public void Update(GameTime gameTime)
    {
        _previousState = _currentState;
        _currentState = Keyboard.GetState();
    }

    public void ClearInputState()
    {
        _currentState = Keyboard.GetState();
        _previousState = _currentState;
    }

    public bool IsActionDown(string action)
    {
        if (!_bindings.TryGetValue(action, out var key))
            return false;
        return _currentState.IsKeyDown(key);
    }

    public bool IsActionPressed(string action)
    {
        if (!_bindings.TryGetValue(action, out var key))
            return false;
        return _currentState.IsKeyDown(key) && !_previousState.IsKeyDown(key);
    }

    public bool IsKeyPressed(Keys key)
    {
        return _currentState.IsKeyDown(key) && !_previousState.IsKeyDown(key);
    }

    public Keys? GetPressedKey()
    {
        var pressedKeys = _currentState.GetPressedKeys();
        var previousKeys = _previousState.GetPressedKeys();

        foreach (var key in pressedKeys)
        {
            if (Array.IndexOf(previousKeys, key) < 0)
                return key;
        }
        return null;
    }

    public Keys GetKey(string action)
    {
        return _bindings.TryGetValue(action, out var key) ? key : Keys.None;
    }

    public Keys GetDefaultKey(string action)
    {
        return _defaultBindings.TryGetValue(action, out var key) ? key : Keys.None;
    }

    public void SetKey(string action, Keys key)
    {
        _bindings[action] = key;
    }

    public string? GetActionForKey(Keys key)
    {
        foreach (var kvp in _bindings)
        {
            if (kvp.Value == key)
                return kvp.Key;
        }
        return null;
    }

    public void ResetToDefaults()
    {
        foreach (var kvp in _defaultBindings)
            _bindings[kvp.Key] = kvp.Value;
    }

    public IReadOnlyDictionary<string, Keys> GetAllBindings()
    {
        return _bindings;
    }

    public IReadOnlyDictionary<string, Keys> GetDefaultBindings()
    {
        return _defaultBindings;
    }
}
