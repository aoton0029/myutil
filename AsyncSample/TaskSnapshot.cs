using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSample
{
    public struct TaskSnapshot
    {
        public string Name { get; init; }
        public TaskState State { get; init; }
        public double? ProgressPercentage { get; init; }
        public string? Message { get; init; }
        public DateTime Timestamp { get; init; }
        public Exception? Error { get; init; }
        public int? SkipCount { get; init; } // ScheduledTaskBase用など
        public string StatusText => State.ToString() +
            (Error != null ? $" (Error: {Error})" : "");
    }
}
