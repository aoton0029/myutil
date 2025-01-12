C#でFlood Fillアルゴリズムを実装する方法を説明します。Flood Fillは、画像編集やゲーム開発で特定の領域を塗りつぶす際に使用されるアルゴリズムです。以下は、C#での基本的な実装例です。

### 1. Flood Fillの概要
Flood Fillは以下のように動作します：
- 指定された座標から開始し、指定された色で塗りつぶします。
- 塗りつぶす際、周囲のピクセルも再帰的またはループで同じ処理を行います。

### 2. 実装方法
以下の例では、2Dグリッドを使用してFlood Fillを実装します。再帰的な方法と、非再帰的な（スタックを使用した）方法の両方を説明します。

---

#### **再帰的Flood Fill**

```csharp
using System;

class Program
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
```

---

#### **非再帰的Flood Fill（スタック使用）**

再帰ではスタックオーバーフローのリスクがあるため、大きなグリッドでは非再帰的な方法が推奨されます。

```csharp
using System;
using System.Collections.Generic;

class Program
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

    static void FloodFill(int[,] image, int startX, int startY, int newColor)
    {
        int rows = image.GetLength(0);
        int cols = image.GetLength(1);
        int targetColor = image[startX, startY];

        if (targetColor == newColor) return;

        Stack<(int, int)> stack = new Stack<(int, int)>();
        stack.Push((startX, startY));

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();

            if (x < 0 || y < 0 || x >= rows || y >= cols || image[x, y] != targetColor)
                continue;

            image[x, y] = newColor;

            stack.Push((x + 1, y)); // 下
            stack.Push((x - 1, y)); // 上
            stack.Push((x, y + 1)); // 右
            stack.Push((x, y - 1)); // 左
        }
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
```

---

### 3. 実行結果
**入力（Before Flood Fill）**
```
1 1 1 0 0
1 1 0 0 0
1 0 0 1 1
0 0 1 1 1
```

**出力（After Flood Fill, newColor = 2）**
```
2 2 2 0 0
2 2 0 0 0
2 0 0 1 1
0 0 1 1 1
```

---

### 4. ポイント
- 再帰的な方法は簡潔ですが、大きな画像ではスタックオーバーフローが発生する可能性があります。
- 非再帰的な方法（スタックやキューを使用）はそのリスクを回避できます。
- 4方向（上下左右）のみでなく、8方向に拡張することも可能です。

必要に応じてカスタマイズしてみてください！