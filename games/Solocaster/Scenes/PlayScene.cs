using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRaycaster;
using Solo;
using Solo.Services;
using Solocaster.Components;
using Solocaster.Persistence;
using System.Linq;

namespace Solocaster.Scenes;

public class PlayScene : Scene
{
    private const int FrameBufferScale = 2;

    public PlayScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();

        var frameBufferWidth = renderService.Graphics.GraphicsDevice.Viewport.Height / FrameBufferScale;
        var frameBufferHeight = renderService.Graphics.GraphicsDevice.Viewport.Width / FrameBufferScale;

        var level = LevelLoader.LoadFromJson("./data/levels/level1.json", Game.Content);
        var map = level.Map;

        var camera = new Camera(map);

        var mainTexture = Game.Content.Load<Texture2D>("wolftextures");
        var textures = mainTexture.Split(64, 64)
            .Select(t => t.Rotate90(RotationDirection.CounterClockwise))
            .ToArray();
        var raycaster = new Raycaster(map, frameBufferWidth, frameBufferHeight, textures);

        var frameTexture = new Texture2D(renderService.Graphics.GraphicsDevice, frameBufferWidth, frameBufferHeight);

        var mapEntity = new GameObject();
        var mapRenderer = new MapRenderer(mapEntity, camera, map, raycaster, frameTexture);
        mapEntity.Components.Add(mapRenderer);
        mapRenderer.LayerIndex = 0;
        Root.AddChild(mapEntity);

        var miniMapEntity = new GameObject();
        var miniMapRenderer = new MiniMapRenderer(miniMapEntity, map, camera);
        miniMapEntity.Components.Add(miniMapRenderer);
        miniMapRenderer.LayerIndex = 1;
        mapEntity.AddChild(miniMapEntity);

        var font = Game.Content.Load<SpriteFont>("Font");

        var debugUIEntity = new GameObject();
        var debugUI = new DebugUIRenderer(debugUIEntity, font, camera);
        debugUIEntity.Components.Add(debugUI);
        debugUI.LayerIndex = 2;
        Root.AddChild(debugUIEntity);
    }
}
