using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework.Input;

namespace Solocaster.Input;

public static class InputActions
{
    public const string MoveForward = "moveForward";
    public const string MoveBackward = "moveBackward";
    public const string RotateLeft = "rotateLeft";
    public const string RotateRight = "rotateRight";
    public const string Run = "run";
    public const string ToggleCombat = "toggleCombat";
    public const string Interact = "interact";
    public const string ToggleCharacterPanel = "toggleCharacterPanel";
    public const string ToggleMinimap = "toggleMinimap";
    public const string ToggleMetrics = "toggleMetrics";
    public const string ToggleDebug = "toggleDebug";
}

public class InputBindings
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

    private readonly Dictionary<string, Keys> _bindings = new();
    private KeyboardState _currentState;
    private KeyboardState _previousState;

    public InputBindings()
    {
        LoadBindings();
    }

    private void LoadBindings()
    {
        // Start with defaults
        foreach (var kvp in DefaultBindings)
            _bindings[kvp.Key] = kvp.Value;

        if (!File.Exists(DefaultPath))
        {
            Console.WriteLine($"InputBindings: Config not found at {DefaultPath}, using defaults");
            return;
        }

        try
        {
            var json = File.ReadAllText(DefaultPath);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (loaded == null) return;

            foreach (var kvp in loaded)
            {
                if (Enum.TryParse<Keys>(kvp.Value, ignoreCase: true, out var key))
                    _bindings[kvp.Key] = key;
                else
                    Console.WriteLine($"InputBindings: Unknown key '{kvp.Value}' for action '{kvp.Key}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InputBindings: Error loading config: {ex.Message}");
        }
    }

    public void Update()
    {
        _previousState = _currentState;
        _currentState = Keyboard.GetState();
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

    public Keys GetKey(string action)
    {
        return _bindings.TryGetValue(action, out var key) ? key : Keys.None;
    }
}
