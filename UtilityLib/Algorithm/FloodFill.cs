using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Algorithm
{
    internal class FloodFill
    {
        static void Main()
        {
            int[,] image = {
            { 1, 1, 1, 0, 0 },
            { 1, 1, 0, 0, 0 },
            { 1, 0, 0, 1, 1 },
            { 0, 0, 1, 1, 1 },
        };

            int startX = 1, startY = 1;
            int newColor = 2;

            Console.WriteLine("Before Flood Fill:");
            PrintImage(image);

            FloodFill(image, startX, startY, newColor);

            Console.WriteLine("\nAfter Flood Fill:");
            PrintImage(image);
        }

        static void FloodFill(int[,] image, int x, int y, int newColor)
        {
            int rows = image.GetLength(0);
            int cols = image.GetLength(1);
            int targetColor = image[x, y];

            if (targetColor == newColor) return;

            Fill(image, x, y, targetColor, newColor, rows, cols);
        }

        static void Fill(int[,] image, int x, int y, int targetColor, int newColor, int rows, int cols)
        {
            if (x < 0 || y < 0 || x >= rows || y >= cols) return;
            if (image[x, y] != targetColor) return;

            image[x, y] = newColor;

            Fill(image, x + 1, y, targetColor, newColor, rows, cols); // 下
            Fill(image, x - 1, y, targetColor, newColor, rows, cols); // 上
            Fill(image, x, y + 1, targetColor, newColor, rows, cols); // 右
            Fill(image, x, y - 1, targetColor, newColor, rows, cols); // 左
        }

        static void PrintImage(int[,] image)
        {
            int rows = image.GetLength(0);
            int cols = image.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(image[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
