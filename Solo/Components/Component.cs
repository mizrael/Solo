using Microsoft.Xna.Framework;

namespace Solo.Components;

public abstract class Component
{
    private bool _initialized = false;

    protected Component(GameObject owner)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public void Init()
    {
        if (_initialized)
            return;

        InitCore();
        _initialized = true;

        OnInitialized?.Invoke();
    }

    protected virtual void InitCore() { }

    public void Update(GameTime gameTime)
    {
        if (!Owner.Enabled)
            return;

        Init();

        UpdateCore(gameTime);
    }

    protected virtual void UpdateCore(GameTime gameTime) { }

    public GameObject Owner { get; }
    public bool Initialized => _initialized;

    public OnInitializedHandler OnInitialized;
    public delegate void OnInitializedHandler();
}
