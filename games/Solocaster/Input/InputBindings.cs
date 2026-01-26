using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework.Input;

namespace Solocaster.Input;

public static class InputBindings
{
    private const string DefaultPath = "./data/settings/keybindings.json";

    private static readonly Dictionary<string, Keys> DefaultBindings = new()
    {
        { InputActions.MoveForward, Keys.W },
        { InputActions.MoveBackward, Keys.S },
        { InputActions.RotateLeft, Keys.A },
        { InputActions.RotateRight, Keys.D },
        { InputActions.Run, Keys.LeftShift },
        { InputActions.ToggleCombat, Keys.R },
        { InputActions.Interact, Keys.E },
        { InputActions.ToggleCharacterPanel, Keys.Tab },
        { InputActions.ToggleMinimap, Keys.M },
        { InputActions.ToggleMetrics, Keys.C },
        { InputActions.ToggleDebug, Keys.L }
    };

    private static readonly Dictionary<string, Keys> Bindings = new();
    private static KeyboardState _currentState;
    private static KeyboardState _previousState;
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
            return;

        foreach (var kvp in DefaultBindings)
            Bindings[kvp.Key] = kvp.Value;

        if (!File.Exists(DefaultPath))
        {
            Console.WriteLine($"InputBindings: Config not found at {DefaultPath}, using defaults");
            _initialized = true;
            return;
        }

        try
        {
            var json = File.ReadAllText(DefaultPath);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (loaded != null)
            {
                foreach (var kvp in loaded)
                {
                    if (Enum.TryParse<Keys>(kvp.Value, ignoreCase: true, out var key))
                        Bindings[kvp.Key] = key;
                    else
                        Console.WriteLine($"InputBindings: Unknown key '{kvp.Value}' for action '{kvp.Key}'");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InputBindings: Error loading config: {ex.Message}");
        }

        _initialized = true;
    }

    public static void Update()
    {
        _previousState = _currentState;
        _currentState = Keyboard.GetState();
    }

    public static bool IsActionDown(string action)
    {
        if (!Bindings.TryGetValue(action, out var key))
            return false;
        return _currentState.IsKeyDown(key);
    }

    public static bool IsActionPressed(string action)
    {
        if (!Bindings.TryGetValue(action, out var key))
            return false;
        return _currentState.IsKeyDown(key) && !_previousState.IsKeyDown(key);
    }

    public static Keys GetKey(string action)
    {
        return Bindings.TryGetValue(action, out var key) ? key : Keys.None;
    }
}
