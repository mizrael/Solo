using Microsoft.Xna.Framework;

namespace Solo.Services;

public sealed class GraphicsDeviceManagerAccessor
{
    private static GraphicsDeviceManagerAccessor? _instance;
    public static GraphicsDeviceManagerAccessor Instance => _instance ??= new GraphicsDeviceManagerAccessor();
    
    private GraphicsDeviceManager? _graphicsDeviceManager;
    public GraphicsDeviceManager GraphicsDeviceManager => _graphicsDeviceManager ?? throw new ApplicationException("Graphics device manager not initialized.");

    public void Initialize(Game game)
    {
        _graphicsDeviceManager = new GraphicsDeviceManager(game);
    }
}