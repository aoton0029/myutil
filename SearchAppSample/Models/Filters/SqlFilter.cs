using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAppSample
{
    public class SqlFilter : IFilter
    {
        public string Column { get; }
        public FilterOperation Operation { get; }
        public object Value { get; }

        public SqlFilter(string column, FilterOperation operation, object value)
        {
            Column = column;
            Operation = operation;
            Value = value;
        }

        public string ToFilter()
        {
            if (Value == null)
            {
                return Operation switch
                {
                    FilterOperation.Equals => $"{Column} IS NULL",
                    FilterOperation.NotEquals => $"{Column} IS NOT NULL",
                    _ => throw new InvalidOperationException("NULL 値の比較は = または != (IS NULL, IS NOT NULL) のみ可能")
                };
            }

            string rowFilterValue = ConvertToRowFilterValue(Value);
            return $"{Column} {GetRowFilterOperator()} {rowFilterValue}";
        }

        public Dictionary<string, object> GetParameters()
        {
            return new Dictionary<string, object>(); // RowFilterではパラメータ不要
        }

        private string GetRowFilterOperator()
        {
            return Operation switch
            {
                FilterOperation.Equals => "=",
                FilterOperation.NotEquals => "<>",
                FilterOperation.GreaterThan => ">",
                FilterOperation.GreaterOrEqual => ">=",
                FilterOperation.LessThan => "<",
                FilterOperation.LessOrEqual => "<=",
                FilterOperation.Like => "LIKE",
                FilterOperation.In => "IN",
                _ => throw new NotImplementedException()
            };
        }

        private string ConvertToRowFilterValue(object value)
        {
            return value switch
            {
                string str => $"'{EscapeRowFilterString(str)}'",  // SQLと同じく ' で囲む
                int or long or float or double or decimal => value.ToString(), // そのまま数値
                bool b => b ? "TRUE" : "FALSE",  // DataView.RowFilter の boolean 表記
                DateTime dt => $"#{dt:yyyy/MM/dd HH:mm:ss}#", // DataView の日付フォーマット
                IEnumerable<object> list when Operation == FilterOperation.In => ConvertListToRowFilter(list),
                _ => throw new NotSupportedException($"Unsupported value type: {value.GetType()}")
            };
        }

        private string EscapeRowFilterString(string input)
        {
            return input.Replace("'", "''"); // シングルクォートをエスケープ
        }

        private string ConvertListToRowFilter(IEnumerable<object> values)
        {
            var formattedValues = values.Select(ConvertToRowFilterValue);
            return $"({string.Join(", ", formattedValues)})";
        }
    }
}
