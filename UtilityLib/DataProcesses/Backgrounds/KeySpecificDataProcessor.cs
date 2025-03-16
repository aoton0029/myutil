using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DataProcesses.Backgrounds
{
    public class KeySpecificDataProcessor
    {
        public string ProcessorKey { get; }
        public DateTime LastProcessingTimestamp { get; private set; }

        public KeySpecificDataProcessor(string key)
        {
            ProcessorKey = key;
            LastProcessingTimestamp = DateTime.UtcNow;
        }

        public void ProcessData(string data)
        {
            LastProcessingTimestamp = DateTime.UtcNow;
            Console.WriteLine($"[{ProcessorKey}] Processed: {data}");
        }
    }
}
