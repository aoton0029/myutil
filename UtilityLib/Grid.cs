using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class Grid<T>
    {
        private T[,] grid;

        public int Rows { get; private set; } // 行数
        public int Columns { get; private set; } // 列数

        public T this[int row, int column]
        {
            get { return GetObject(row, column); }
            set { SetObject(row, column, value); }
        }

        public Grid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            grid = new T[rows, columns];
        }

        public void SetObject(int row, int column, T obj)
        {
            if (row >= 0 && row < Rows && column >= 0 && column < Columns)
            {
                grid[row, column] = obj;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Invalid row or column index.");
            }
        }

        public T GetObject(int row, int column)
        {
            if (row >= 0 && row < Rows && column >= 0 && column < Columns)
            {
                return grid[row, column];
            }
            else
            {
                throw new ArgumentOutOfRangeException("Invalid row or column index.");
            }
        }

        
    }

}
