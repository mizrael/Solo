using Solo;
using Solo.Assets;
using Solo.Components;
using System;

namespace Solocaster.Components;

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
}
