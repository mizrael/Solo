using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solocaster.Input;
using Solocaster.UI;

namespace Solocaster.Components;

public class PlayerUIController : Component
{
    public CharacterPanel? CharacterPanel { get; set; }
    public MetricsPanel? MetricsPanel { get; set; }
    public GameObject? MiniMapEntity { get; set; }
    public GameObject? DebugUIEntity { get; set; }

    public PlayerUIController(GameObject owner) : base(owner)
    {
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (InputBindings.IsActionPressed(InputActions.ToggleCharacterPanel))
            CharacterPanel?.Toggle();

        if (InputBindings.IsActionPressed(InputActions.ToggleMinimap) && MiniMapEntity != null)
            MiniMapEntity.Enabled = !MiniMapEntity.Enabled;

        if (InputBindings.IsActionPressed(InputActions.ToggleDebug) && DebugUIEntity != null)
            DebugUIEntity.Enabled = !DebugUIEntity.Enabled;

        if (InputBindings.IsActionPressed(InputActions.ToggleMetrics))
            MetricsPanel?.Toggle();
    }
}
