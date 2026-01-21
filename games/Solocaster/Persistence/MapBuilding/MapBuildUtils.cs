namespace Solocaster.Persistence.MapBuilding;

public static class MapBuildUtils
{
    public static void EnsurePerimeterClosed(int[][] cells)
    {
        if (cells == null || cells.Length == 0)
            return;

        int height = cells.Length;
        int width = cells[0].Length;

        for (int col = 0; col < width; col++)
        {
            cells[0][col] = 1;
            cells[height - 1][col] = 1;
        }

        for (int row = 0; row < height; row++)
        {
            cells[row][0] = 1;
            cells[row][width - 1] = 1;
        }
    }
}
