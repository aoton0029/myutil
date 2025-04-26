using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAppSample
{

    public interface IFilter
    {
        string ToFilter(); // WHERE句を生成
        Dictionary<string, object> GetParameters(); // SQLパラメータ
    }

    public enum FilterOperation
    {
        Equals,         // "="
        NotEquals,      // "<>"
        GreaterThan,    // ">"
        GreaterOrEqual, // ">="
        LessThan,       // "<"
        LessOrEqual,    // "<="
        Like,           // "LIKE"
        In              // "IN"
    }

    public enum FilterLogic
    {
        And,
        Or
    }
}
