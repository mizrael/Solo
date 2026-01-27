using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo.Components;
using Solocaster.Components;
using Solocaster.Entities;
using Solocaster.Inventory;
using System;

namespace Solocaster.AI.Player;

public class PlayerStateContext
{
    public required InventoryComponent Inventory { get; init; }
    public required StatsComponent Stats { get; init; }

    public MouseState PreviousMouseState { get; set; }
    public float CurrentMoveSpeed { get; set; }

    public PlayerState PreviousStateBeforeRun { get; set; } = PlayerState.Exploring;

    public float SpeedMultiplier { get; set; } = 1.0f;

    public float LeftHandRaiseAmount { get; set; }
    public float RightHandRaiseAmount { get; set; }
}
