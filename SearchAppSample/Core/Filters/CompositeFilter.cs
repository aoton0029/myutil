using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAppSample
{
    public class CompositeFilter : IFilter
    {
        public List<IFilter> Filters { get; } = new List<IFilter>();
        public FilterLogic Logic { get; }

        public CompositeFilter(FilterLogic logic)
        {
            Logic = logic;
        }

        public void AddFilter(IFilter filter)
        {
            Filters.Add(filter);
        }

        public string ToFilter()
        {
            if (!Filters.Any()) return "";

            var whereClauses = Filters.Select(f => $"({f.ToFilter()})");
            string separator = Logic == FilterLogic.And ? " AND " : " OR ";
            return string.Join(separator, whereClauses);
        }

        public Dictionary<string, object> GetParameters()
        {
            return new Dictionary<string, object>(); // RowFilterではパラメータ不要
        }
    }

}
