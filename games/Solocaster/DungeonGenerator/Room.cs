using System.Drawing;

namespace Solocaster.DungeonGenerator;

public class Room : Map
{
    #region Constructors

    public Room(int width, int height) : base(width, height)
    {
    }

    #endregion

    #region Methods

    public void InitializeRoomCells()
    {
        foreach (Point location in CellLocations)
        {
            Cell cell = new Cell();

            cell.WestSide = (location.X == bounds.X) ? SideType.Wall : SideType.Empty;
            cell.EastSide = (location.X == bounds.Width - 1) ? SideType.Wall : SideType.Empty;
            cell.NorthSide = (location.Y == bounds.Y) ? SideType.Wall : SideType.Empty;
            cell.SouthSide = (location.Y == bounds.Height - 1) ? SideType.Wall : SideType.Empty;

            this[location] = cell;
        }
    }

    public void SetLocation(Point location)
    {
        bounds = new Rectangle(location, bounds.Size);
    }

    /// Get the room min and max location in tile coordinates
    public (Point minPoint, Point maxPoint) GetBounds(int tileStep)
    {
        Point minPoint = new Point(tileStep * (Bounds.Location.X * 2 + 1), tileStep * (Bounds.Location.Y * 2 + 1));
        Point maxPoint = new Point(tileStep * Bounds.Right * 2, tileStep * Bounds.Bottom * 2);

        return (minPoint, maxPoint);
    }

    public (int row, int col) GetRandomTile(int tileStep)
    {
        var (min, max) = GetBounds(tileStep);
        var row = Random.Instance.Next(min.X, max.X);
        var col = Random.Instance.Next(min.Y, max.Y);
        return (row, col);
    }

    #endregion
}