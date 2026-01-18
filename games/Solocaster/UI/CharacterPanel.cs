using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solocaster.Components;
using Solocaster.UI.Widgets;

namespace Solocaster.UI;

public class CharacterPanel : Widget
{
    private const int PanelSpacing = 10;

    private readonly StatsPanel _statsPanel;
    private readonly InventoryPanel _inventoryPanel;
    private readonly DragDropManager _dragDropManager;

    public DragDropManager DragDropManager => _dragDropManager;

    public CharacterPanel(
        InventoryComponent inventory,
        StatsComponent stats,
        SpriteFont font,
        Game game)
    {
        _dragDropManager = new DragDropManager();
        _statsPanel = new StatsPanel(stats, font);
        _inventoryPanel = new InventoryPanel(inventory, stats, _dragDropManager, font, game);

        // Position stats panel on the left
        _statsPanel.Position = Vector2.Zero;

        // Position inventory panel to the right of stats panel
        _inventoryPanel.Position = new Vector2(_statsPanel.Size.X + PanelSpacing, 0);

        // Calculate total size
        Size = new Vector2(
            _statsPanel.Size.X + PanelSpacing + _inventoryPanel.Size.X,
            MathHelper.Max(_statsPanel.Size.Y, _inventoryPanel.Size.Y)
        );

        AddChild(_statsPanel);
        AddChild(_inventoryPanel);

        Visible = false;
    }

    public void Toggle()
    {
        Visible = !Visible;
    }

    public void CenterOnScreen(int screenWidth, int screenHeight)
    {
        Position = new Vector2(
            (screenWidth - Size.X) / 2,
            (screenHeight - Size.Y) / 2
        );
    }
}
