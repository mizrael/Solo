using Microsoft.Xna.Framework;
using Pacman.Scenes;
using Solo;
using Solo.AI;
using Solo.Assets;
using Solo.Assets.Loaders;
using Solo.Components;
using Solo.Services;
using System;

namespace Pacman.Components;

public class GhostBrainComponent : Component
{
    private StateMachine _logic;

    private AnimatedSpriteSheet _walkAnim;
    private AnimatedSpriteSheet _scaredAnim1;
    private AnimatedSpriteSheet _scaredAnim2;

    public GhostBrainComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        var mapLogic = Map.Components.Get<MapLogicComponent>();
        var currTile = mapLogic.GetGhostStartTile(this.GhostType);

        var transform = Owner.Components.Get<TransformComponent>();
        transform.Local.Position = mapLogic.GetTileCenter(currTile);

        var renderer = Owner.Components.Get<AnimatedSpriteSheetRenderer>();
        var bbox = Owner.Components.Add<BoundingBoxComponent>();

        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();
        var calculateSize = new Action(() =>
        {
            if (renderer.CurrentFrame is null)
                return;

            transform.Local.Scale.X = mapLogic.TileSize.X / renderer.CurrentFrame.Bounds.Width;
            transform.Local.Scale.Y = mapLogic.TileSize.Y / renderer.CurrentFrame.Bounds.Height;

            var bboxSize = new Point(
                 (int)((float)renderer.CurrentFrame.Bounds.Size.X * transform.Local.Scale.X),
                 (int)((float)renderer.CurrentFrame.Bounds.Size.Y * transform.Local.Scale.Y));
            bbox.SetSize(bboxSize);
        });
        calculateSize();

        renderService.Window.ClientSizeChanged += (s, e) => calculateSize();

        renderer.OnAnimationSet += _ => calculateSize();

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
        this.State = GhostStates.Idle;

        _logic = ghostType switch
        {
            GhostTypes.Blinky => AI.StateMachines.Blinky(this.Owner, player, map),
            GhostTypes.Inky => AI.StateMachines.Inky(this.Owner, player, map, playScene),
            GhostTypes.Pinky => AI.StateMachines.Pinky(this.Owner, player, map),
            GhostTypes.Clyde => AI.StateMachines.Clyde(this.Owner, player, map),
            _ => throw new NotImplementedException()
        };

        var ghostName = GhostType.ToString().ToLower();
        _walkAnim = AnimatedSpriteSheetLoader.Load($"meta/animations/{ghostName}_walk.json", playScene.Game);
        _scaredAnim1 = AnimatedSpriteSheetLoader.Load("meta/animations/ghost_frightened1.json", playScene.Game);
        _scaredAnim2 = AnimatedSpriteSheetLoader.Load("meta/animations/ghost_frightened2.json", playScene.Game);
    }

    public void SetAnimation(GhostAnimations animType)
    { 
        var renderer = Owner.Components.Get<AnimatedSpriteSheetRenderer>();
        renderer.Animation = animType switch
        {
            GhostAnimations.Walk => _walkAnim,
            GhostAnimations.Scared1 => _scaredAnim1,
            GhostAnimations.Scared2 => _scaredAnim2,
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