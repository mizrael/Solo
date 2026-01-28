using Microsoft.Xna.Framework;

namespace Solo.Services;

public sealed class SceneManager
{
    private static readonly Lazy<SceneManager> _instance = new(() => new SceneManager());
    public static SceneManager Instance => _instance.Value;

    private SceneManager() { }

    private readonly Dictionary<string, Scene> _scenes = new();
    private readonly Stack<Scene> _sceneStack = new();

    public Scene? Current => _sceneStack.Count > 0 ? _sceneStack.Peek() : null;

    public void AddScene(string name, Scene scene)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));
        _scenes.Add(name, scene);
    }

    public void SetScene(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (!_scenes.ContainsKey(name))
            throw new ArgumentOutOfRangeException(nameof(name), $"Invalid scene name: '{name}'");

        while (_sceneStack.Count > 0)
            PopScene();

        PushScene(name);
    }

    public void PushScene(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (!_scenes.TryGetValue(name, out var scene))
            throw new ArgumentOutOfRangeException(nameof(name), $"Invalid scene name: '{name}'");

        _sceneStack.Push(scene);
        scene.Enter();
        OnSceneChanged?.Invoke(scene);
    }

    public void PopScene()
    {
        if (_sceneStack.Count == 0)
            return;

        var scene = _sceneStack.Pop();
        scene.Exit();

        if (_sceneStack.Count > 0)
            OnSceneChanged?.Invoke(_sceneStack.Peek());
    }

    public void Step(GameTime gameTime)
    {
        Current?.Update(gameTime);
    }

    public void Render()
    {
        Current?.Render();
    }

    public event OnSceneChangedHandler? OnSceneChanged;
    public delegate void OnSceneChangedHandler(Scene currentScene);
}
