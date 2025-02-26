using Microsoft.Xna.Framework;
using Solo;
using Solo.AI;
using Solo.Components;
using Solo.Services;
using System;

namespace Pacman.Components;

public class GhostBrainComponent : Component
{
    public StateMachine Logic;

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
        Logic.Update(gameTime);
    }

    public GameObject Map;

    public Ghosts GhostType;

    public GameObject Player;
}