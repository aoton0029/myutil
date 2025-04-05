using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChartSample
{
    static class Utils
    {
        public static void To<T>(this T value, ref T original, Action<string> updated, string propertyName)
        {
            if (original.Equals(value)) return;

            var backup = original;
            original = value;

            try
            {
                updated(propertyName);
            }
            catch
            {
                original = backup;
                throw;
            }
        }

        public static List<KeyValuePair<string, string>> ToDataSource<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                       .Cast<T>()
                       .Select(e => new KeyValuePair<string, string>(e.ToString(), e.ToString()))
                       .ToList();
        }

        public static void SetEnumDataSource<T>(this DataGridViewComboBoxColumn column) where T : Enum
        {
            column.DataSource = ToDataSource<T>();
            column.DisplayMember = "Key";
            column.ValueMember = "Value";
        }
    }
}
