using Microsoft.Xna.Framework;

namespace Solo.Services;

public abstract class Scene
{
    public Game Game { get; }
    public GameServicesCollection Services { get; } = new();

    protected SceneObjectsGraph _objectsGraph;
    public SceneObjectsGraph ObjectsGraph => _objectsGraph;

    protected RenderService _renderService;

    private bool _initialized = false;

    protected Scene(Game game)
    {
        Game = game ?? throw new ArgumentNullException(nameof(game));
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
        
        _objectsGraph = new SceneObjectsGraph();
        Services.Add(_objectsGraph);

        _renderService = new RenderService(this.Game.GraphicsDevice);
        Services.Add(_renderService);

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
        _renderService.Render();
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
