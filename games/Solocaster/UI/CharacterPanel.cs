using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solocaster.Components;
using Solocaster.UI.Widgets;

namespace Solocaster.UI;

public class CharacterPanel : Widget
{
    private const int PanelSpacing = 10;
    private const int BeltTopMargin = 15;

    private readonly StatsPanel _statsPanel;
    private readonly InventoryPanel _inventoryPanel;
    private readonly BeltPanel _beltPanel;

    public CharacterPanel(
        InventoryComponent inventory,
        StatsComponent stats,
        DragDropManager dragDropManager,
        SpriteFont font,
        Game game)
    {
        _statsPanel = new StatsPanel(stats, font, game);
        _inventoryPanel = new InventoryPanel(inventory, dragDropManager, font, game);
        _beltPanel = new BeltPanel(inventory, dragDropManager, font, game);

        _statsPanel.Position = Vector2.Zero;
        _inventoryPanel.Position = new Vector2(_statsPanel.Size.X + PanelSpacing, 0);

        float topRowHeight = MathHelper.Max(_statsPanel.Size.Y, _inventoryPanel.Size.Y);

        _beltPanel.Position = new Vector2(
            _inventoryPanel.Position.X + (_inventoryPanel.Size.X - _beltPanel.Size.X) / 2,
            topRowHeight + BeltTopMargin
        );

        float totalHeight = _beltPanel.Position.Y + _beltPanel.Size.Y;

        Size = new Vector2(
            _statsPanel.Size.X + PanelSpacing + _inventoryPanel.Size.X,
            totalHeight
        );

        AddChild(_statsPanel);
        AddChild(_inventoryPanel);
        AddChild(_beltPanel);

        Visible = false;
    }

    public void CenterOnScreen(int screenWidth, int screenHeight)
    {
        Position = new Vector2(
            (screenWidth - Size.X) / 2,
            (screenHeight - Size.Y) / 2
        );
    }
}
