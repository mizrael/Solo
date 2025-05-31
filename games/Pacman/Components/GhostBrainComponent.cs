using Microsoft.Xna.Framework;
using Pacman.Scenes;
using Solo;
using Solo.AI;
using Solo.Assets.Loaders;
using Solo.Components;
using Solo.Services;
using System;

namespace Pacman.Components;

public class GhostBrainComponent : Component
{
    private StateMachine _logic;

    public GhostBrainComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        var mapLogic = Map.Components.Get<MapLogicComponent>();

        var currTile = mapLogic.GetGhostStartTile(this.GhostType);
        var transform = Owner.Components.Get<TransformComponent>();

        var renderer = Owner.Components.Get<AnimatedSpriteSheetRenderer>();
        var bbox = Owner.Components.Add<BoundingBoxComponent>();

        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();
        var calculateSize = new Action(() =>
        {
            if (renderer.CurrentFrame is null)
                return;

            transform.Local.Scale.X = mapLogic.TileSize.X / renderer.CurrentFrame.Bounds.Width;
            transform.Local.Scale.Y = mapLogic.TileSize.Y / renderer.CurrentFrame.Bounds.Height;

            transform.Local.Position = mapLogic.GetTileCenter(currTile);

            var bboxSize = new Point(
                 (int)((float)renderer.CurrentFrame.Bounds.Size.X * transform.Local.Scale.X),
                 (int)((float)renderer.CurrentFrame.Bounds.Size.Y * transform.Local.Scale.Y));
            bbox.SetSize(bboxSize);
        });
        calculateSize();

        renderService.Window.ClientSizeChanged += (s, e) => calculateSize();

        base.InitCore();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        _logic.Update(gameTime);
    }

    public void Setup(
        PlayScene playScene,
        GhostTypes ghostType, 
        GameObject map, 
        GameObject player)
    {
        this.Map = map;
        this.Player = player;
        this.GhostType = ghostType;
        _logic = ghostType switch
        {
            GhostTypes.Blinky => AI.StateMachines.Blinky(playScene.Game, this.Owner, player, map),
            GhostTypes.Inky => AI.StateMachines.Inky(playScene.Game, this.Owner, player, map, playScene),
            GhostTypes.Pinky => AI.StateMachines.Pinky(playScene.Game, this.Owner, player, map),
            GhostTypes.Clyde => AI.StateMachines.Clyde(playScene.Game, this.Owner, player, map),
            _ => throw new NotImplementedException()
        };
    }

    public void SetAnimation(GhostAnimations animType, Game game)
    {
        var ghostName = this.Owner.Components.Get<GhostBrainComponent>().GhostType.ToString().ToLower();
        var renderer = Owner.Components.Get<AnimatedSpriteSheetRenderer>();
        renderer.Animation = animType switch
        {
            GhostAnimations.Walk => AnimatedSpriteSheetLoader.Load($"meta/animations/{ghostName}_walk.json", game),
            GhostAnimations.Scared1 => AnimatedSpriteSheetLoader.Load("meta/animations/ghost_frightened1.json", game),
            GhostAnimations.Scared2 => AnimatedSpriteSheetLoader.Load("meta/animations/ghost_frightened2.json", game),
            _ => throw new NotImplementedException()
        };
    }

    public void WasEaten()
    {
        var mapLogic = this.Map.Components.Get<MapLogicComponent>();
        var ghostStartTile = mapLogic.GetGhostStartTile(this.GhostType);
        var ghostTransform = this.Owner.Components.Get<TransformComponent>();
        ghostTransform.Local.Position = mapLogic.GetTileCenter(ghostStartTile);

        this.State = GhostStates.Idle;
        _logic.Reset();
    }

    public GameObject Map { get; private set; }

    public GhostTypes GhostType { get; private set; }

    public GameObject Player { get; private set; }

    public GhostStates State { get; set; }
}