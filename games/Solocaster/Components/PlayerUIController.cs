using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solo.Services;
using Solocaster.Scenes;
using Solocaster.Services;

namespace Solocaster.Components;

public class PlayerUIController : Component
{
    private readonly InputService _inputService;

    public GameObject? MiniMapEntity { get; set; }
    public GameObject? DebugUIEntity { get; set; }

    public PlayerUIController(GameObject owner, InputService inputService) : base(owner)
    {
        _inputService = inputService;
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (_inputService.IsActionPressed(InputActions.ToggleCharacterPanel))
            SceneManager.Instance.PushScene(SceneNames.CharacterPanel);

        if (_inputService.IsActionPressed(InputActions.ToggleMinimap) && MiniMapEntity != null)
            MiniMapEntity.Enabled = !MiniMapEntity.Enabled;

        if (_inputService.IsActionPressed(InputActions.ToggleDebug) && DebugUIEntity != null)
            DebugUIEntity.Enabled = !DebugUIEntity.Enabled;

        if (_inputService.IsActionPressed(InputActions.ToggleMetrics))
            SceneManager.Instance.PushScene(SceneNames.MetricsPanel);
    }
}
