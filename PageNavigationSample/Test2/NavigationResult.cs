using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Test2
{
    public class NavigationResult
    {
        public bool IsCompleted { get; }
        public object? Data { get; }

        private NavigationResult(bool completed, object? data)
        {
            IsCompleted = completed;
            Data = data;
        }

        public static NavigationResult Completed(object? data = null) => new(true, data);
        public static NavigationResult Cancelled(object? reason = null) => new(false, reason);
    }
}
