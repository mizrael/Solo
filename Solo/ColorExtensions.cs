using Microsoft.Xna.Framework;

namespace Solo;

public static class ColorExtensions
{
    public static Color[] Rotate90(this Color[] originalData, int width, int height, RotationDirection direction = RotationDirection.CounterClockwise)
    {
        var rotatedData = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int newX, newY;

                if (direction == RotationDirection.Clockwise)
                {
                    newX = height - 1 - y;
                    newY = x;
                }
                else
                {
                    newX = y;
                    newY = width - 1 - x;
                }

                int originalIndex = y * width + x;
                int rotatedIndex = newY * height + newX;

                rotatedData[rotatedIndex] = originalData[originalIndex];
            }
        }

        return rotatedData;
    }
}
