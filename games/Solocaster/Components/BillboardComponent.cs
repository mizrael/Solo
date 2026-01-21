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
    public BillboardComponent(GameObject owner, IFrameProvider frameProvider) : base(owner)
    {
        FrameProvider = frameProvider ?? throw new ArgumentNullException(nameof(frameProvider));
    }

    public IFrameProvider FrameProvider { get; }

    public Vector2 Scale { get; set; } = Vector2.One;
    public BillboardAnchor Anchor { get; set; } = BillboardAnchor.Center;
}
