using Microsoft.Xna.Framework;

namespace Solo.Services;

public class SceneManager : IGameService
{
    private readonly Dictionary<string, Scene> _scenes = new();

    public void AddScene(string name, Scene scene)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (null == scene)
            throw new ArgumentNullException(nameof(scene));
        _scenes.Add(name, scene);
    }

    public void SetCurrentScene(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (!_scenes.ContainsKey(name))
            throw new ArgumentOutOfRangeException(nameof(name), $"invalid scene name: '{name}'");
        if (this.Current is not null)
            this.Current.Exit();

        this.Current = _scenes[name];

        this.Current.Enter();

        this.OnSceneChanged?.Invoke(this.Current);
    }

    public void Step(GameTime gameTime)
    {
        if (this.Current is not null)
            this.Current.Step(gameTime);
    }

    public Scene Current { get; private set; }

    public event OnSceneChangedHandler OnSceneChanged;
    public delegate void OnSceneChangedHandler(Scene currentScene);
}
