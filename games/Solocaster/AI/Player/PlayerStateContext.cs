using Microsoft.Xna.Framework.Input;
using Solocaster.Components;
using Solocaster.Inventory;

namespace Solocaster.AI.Player;

public class PlayerStateContext
{
    public required InventoryComponent Inventory { get; init; }
    public required StatsComponent Stats { get; init; }

    public MouseState PreviousMouseState { get; set; }
    public float CurrentMoveSpeed { get; set; }

    public Solo.AI.State? StateBeforeRun { get; set; }

    // State configuration (set by states in OnEnter)
    public bool ShowsHands { get; set; }
    public float SpeedMultiplier { get; set; } = 1.0f;
    public float BobSpeed { get; set; } = 1.5f;

    public float LeftHandRaiseAmount { get; set; }
    public float RightHandRaiseAmount { get; set; }
}
