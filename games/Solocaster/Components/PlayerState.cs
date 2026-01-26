namespace Solocaster.Components;

public enum PlayerState
{
    Exploring,  // Default - hands lowered, normal walk speed
    Combat,     // Hands raised, ready for action
    Running,    // Fast movement, hands visible, draining stamina
    Exhausted   // Recovery after stamina depletion, reduced speed
}
