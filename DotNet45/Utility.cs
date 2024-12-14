using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet45
{
    internal class Utility
    {
        public static void Load()
        {
            DataTable table = new DataTable();
            try
            {
                table.BeginLoadData();

                for (int i = 0; i < 100000; i++)
                {
                    object[] row = { i, $"Name_{i}", 20 + (i % 50) };
                    table.LoadDataRow(row, true);
                }
            }
            finally
            {
                table.EndLoadData();
            }
        }
    }
}
