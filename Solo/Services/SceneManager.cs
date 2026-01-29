using Microsoft.Xna.Framework;

namespace Solo.Services;

public sealed class SceneManager
{
    private static readonly Lazy<SceneManager> _instance = new(() => new SceneManager());
    public static SceneManager Instance => _instance.Value;

    private SceneManager() { }

    private readonly Dictionary<string, Func<Scene>> _scenes = new();
    private readonly Stack<Scene> _sceneStack = new();

    public Scene? Current => _sceneStack.Count > 0 ? _sceneStack.Peek() : null;

    public void AddScene(string name, Scene scene)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(scene);
        this.AddScene(name, () => scene);
    }

    public void AddScene<TScene>(string name, Game game) 
        where TScene : Scene
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(game);

        this.AddScene(name, () => (TScene)Activator.CreateInstance(typeof(TScene), game)!);
    }

    public void AddScene(string name, Func<Scene> sceneFactory)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(sceneFactory);

        _scenes.Add(name, sceneFactory);
    }

    /// <summary>
    /// removes the all the scenes from the stack and sets the specified scene as the current scene
    /// </summary>
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

    /// <summary>
    /// pushes a new scene onto the scene stack. The previous scene remains in the stack but is no longer active.
    /// <see cref="PopScene"/> can be called to return to the previous scene.
    /// </summary>
    public void PushScene(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (!_scenes.TryGetValue(name, out var sceneFactory))
            throw new ArgumentOutOfRangeException(nameof(name), $"Invalid scene name: '{name}'");

        var scene = sceneFactory();

        _sceneStack.Push(scene);
        scene.Enter();
        OnSceneChanged?.Invoke(scene);
    }

    /// <summary>
    /// Removes the current scene from the scene stack and transitions to the previous scene, if one exists.
    /// </summary>
    /// <remarks>
    /// If the scene stack is empty, this method does nothing. After removing the current scene, the method invokes the
    /// scene changed event with the new top scene, if available.
    /// </remarks>
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
