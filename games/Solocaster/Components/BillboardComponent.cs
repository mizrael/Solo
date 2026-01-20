using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using System;

namespace Solocaster.Components;

public enum BillboardAnchor
{
    Bottom,
    Center,
    Top
}

public class BillboardComponent : Component
{
    private IFrameProvider _frameProvider;

    private BillboardComponent(GameObject owner) : base(owner)
    {
    }

    public IFrameProvider FrameProvider
    {
        get => _frameProvider;
        set => _frameProvider = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Vector2 Scale { get; set; } = Vector2.One;
    public BillboardAnchor Anchor { get; set; } = BillboardAnchor.Center;
}
