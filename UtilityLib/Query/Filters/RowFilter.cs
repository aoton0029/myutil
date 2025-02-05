using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Query.Filters
{
    public class Filter : IFilter
    {
        public string Column { get; }
        public FilterOperation Operation { get; }
        public object Value { get; }

        public Filter(string column, FilterOperation operation, object value)
        {
            Column = column;
            Operation = operation;
            Value = value;
        }

        public string ToRowFilter()
        {
            string filterValue = ConvertToRowFilterValue(Value);
            return $"{Column} {GetRowFilterOperator()} {filterValue}";
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
                FilterOperation.In => "", // `IN` は `OR` に変換するのでここでは使用しない
                _ => throw new NotImplementedException()
            };
        }

        private string ConvertToRowFilterValue(object value)
        {
            return value switch
            {
                null => "NULL",
                string str => $"'{EscapeRowFilterString(str)}'", // LIKE などの文字列フィルター
                int or long or float or double or decimal => value.ToString(),
                bool b => b ? "TRUE" : "FALSE", // `RowFilter` では `TRUE` / `FALSE`
                DateTime dt => $"#{dt:yyyy/MM/dd HH:mm:ss}#", // `RowFilter` の日付フォーマット
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
            return string.Join(" OR ", values.Select(v => $"{Column} = {ConvertToRowFilterValue(v)}"));
        }

        public string ToFilter()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetParameters()
        {
            throw new NotImplementedException();
        }
    }

}
