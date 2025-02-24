using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solo.Services;
using System;

namespace Pacman.Components;

public class GhostBrainComponent : Component
{
    private TransformComponent _transform;
    private MapLogicComponent _mapLogic;
    private int _currRow = -1;
    private int _currCol = -1;
    private Directions _direction;


    public GhostBrainComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _mapLogic = Map.Components.Get<MapLogicComponent>();

        (_currRow, _currCol) = _mapLogic.GetGhostStartTile(this.GhostType);
        _transform = Owner.Components.Get<TransformComponent>();
        _transform.Local.Position = _mapLogic.GetTileCenter(_currRow, _currCol);

        var renderer = Owner.Components.Get<AnimatedSpriteSheetRenderer>();
        var bbox = Owner.Components.Add<BoundingBoxComponent>();

        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();
        var calculateSize = new Action(() =>
        {
            if (renderer.CurrentFrame is null)
                return;

            _transform.Local.Scale.X = _mapLogic.TileSize.X / renderer.CurrentFrame.Bounds.Width;
            _transform.Local.Scale.Y = _mapLogic.TileSize.Y / renderer.CurrentFrame.Bounds.Height;

            _transform.Local.Position = _mapLogic.GetTileCenter(_currRow, _currCol);

            var bboxSize = new Point(
                 (int)((float)renderer.CurrentFrame.Bounds.Size.X * _transform.Local.Scale.X),
                 (int)((float)renderer.CurrentFrame.Bounds.Size.Y * _transform.Local.Scale.Y));
            bbox.SetSize(bboxSize);
        });
        calculateSize();

        renderService.Window.ClientSizeChanged += (s, e) => calculateSize();

        base.InitCore();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
      
    }

    public float Speed = .1f;

    public GameObject Map;

    public Ghosts GhostType;
}