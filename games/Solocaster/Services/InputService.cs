using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo.Services;

namespace Solocaster.Services;

//TODO: refactor to support bindings based on scene
public class InputService : IGameService
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

    public InputService()
    {
        foreach (var kvp in DefaultBindings)
            _bindings[kvp.Key] = kvp.Value;

        LoadBindings();
    }

    private void LoadBindings()
    {
        if (!File.Exists(DefaultPath))
        {
            Console.WriteLine($"InputService: Config not found at {DefaultPath}, using defaults");
            return;
        }

        try
        {
            var json = File.ReadAllText(DefaultPath);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (loaded == null)
                return;

            foreach (var kvp in loaded)
            {
                if (Enum.TryParse<Keys>(kvp.Value, ignoreCase: true, out var key))
                    _bindings[kvp.Key] = key;
                else
                    Console.WriteLine($"InputService: Unknown key '{kvp.Value}' for action '{kvp.Key}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InputService: Error loading config: {ex.Message}");
        }
    }

    public void Update(GameTime gameTime)
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
