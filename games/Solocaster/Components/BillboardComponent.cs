using Microsoft.Xna.Framework;
using Solo;
using Solo.Assets;
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
    private Sprite _sprite;

    private BillboardComponent(GameObject owner) : base(owner)
    {
    }

    public Sprite Sprite
    {
        get => _sprite;
        set => _sprite = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Vector2 Scale { get; set; } = Vector2.One;
    public BillboardAnchor Anchor { get; set; } = BillboardAnchor.Center;
}
