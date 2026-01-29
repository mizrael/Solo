using Microsoft.Xna.Framework;

namespace Solo.Services;

public abstract class Scene
{
    public Game Game { get; }
    
    public readonly GameServicesCollection Services = new();

    public readonly SceneObjectsGraph ObjectsGraph = new();

    public readonly RenderService RenderService;

    private bool _initialized = false;

    protected Scene(Game game)
    {
        Game = game ?? throw new ArgumentNullException(nameof(game));

        Services.Add(ObjectsGraph);

        RenderService = new RenderService(this.Game.GraphicsDevice);
        Services.Add(RenderService);
    }

    public void Enter()
    {
        Initialize();
        EnterCore();
    }

    private void Initialize()
    {
        if(_initialized)
            return; 

        InitializeCore();

        _initialized = true;
    }

    public void Update(GameTime gameTime)
    {
        Services.Step(gameTime);

        UpdateCore(gameTime);
    }

    public void Render()
    {
        RenderService.Render();
        RenderCore();
    }

    public void Exit()
    {
        ExitCore();
    }

    protected virtual void InitializeCore() { }
    protected virtual void EnterCore() { }
    protected virtual void ExitCore() { }
    protected virtual void UpdateCore(GameTime gameTime) { }
    protected virtual void RenderCore() { }
}
