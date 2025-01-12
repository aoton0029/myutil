C#��Flood Fill�A���S���Y��������������@��������܂��BFlood Fill�́A�摜�ҏW��Q�[���J���œ���̗̈��h��Ԃ��ۂɎg�p�����A���S���Y���ł��B�ȉ��́AC#�ł̊�{�I�Ȏ�����ł��B

### 1. Flood Fill�̊T�v
Flood Fill�͈ȉ��̂悤�ɓ��삵�܂��F
- �w�肳�ꂽ���W����J�n���A�w�肳�ꂽ�F�œh��Ԃ��܂��B
- �h��Ԃ��ہA���͂̃s�N�Z�����ċA�I�܂��̓��[�v�œ����������s���܂��B

### 2. �������@
�ȉ��̗�ł́A2D�O���b�h���g�p����Flood Fill���������܂��B�ċA�I�ȕ��@�ƁA��ċA�I�ȁi�X�^�b�N���g�p�����j���@�̗�����������܂��B

---

#### **�ċA�IFlood Fill**

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

        Fill(image, x + 1, y, targetColor, newColor, rows, cols); // ��
        Fill(image, x - 1, y, targetColor, newColor, rows, cols); // ��
        Fill(image, x, y + 1, targetColor, newColor, rows, cols); // �E
        Fill(image, x, y - 1, targetColor, newColor, rows, cols); // ��
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

#### **��ċA�IFlood Fill�i�X�^�b�N�g�p�j**

�ċA�ł̓X�^�b�N�I�[�o�[�t���[�̃��X�N�����邽�߁A�傫�ȃO���b�h�ł͔�ċA�I�ȕ��@����������܂��B

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

            stack.Push((x + 1, y)); // ��
            stack.Push((x - 1, y)); // ��
            stack.Push((x, y + 1)); // �E
            stack.Push((x, y - 1)); // ��
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

### 3. ���s����
**���́iBefore Flood Fill�j**
```
1 1 1 0 0
1 1 0 0 0
1 0 0 1 1
0 0 1 1 1
```

**�o�́iAfter Flood Fill, newColor = 2�j**
```
2 2 2 0 0
2 2 0 0 0
2 0 0 1 1
0 0 1 1 1
```

---

### 4. �|�C���g
- �ċA�I�ȕ��@�͊Ȍ��ł����A�傫�ȉ摜�ł̓X�^�b�N�I�[�o�[�t���[����������\��������܂��B
- ��ċA�I�ȕ��@�i�X�^�b�N��L���[���g�p�j�͂��̃��X�N������ł��܂��B
- 4�����i�㉺���E�j�݂̂łȂ��A8�����Ɋg�����邱�Ƃ��\�ł��B

�K�v�ɉ����ăJ�X�^�}�C�Y���Ă݂Ă��������I