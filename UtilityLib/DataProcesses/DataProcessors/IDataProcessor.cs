using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DataProcesses
{
    public interface IDataProcessor
    {
        Task ScheduleDataProcessing(DataWithKey dataWithKey);
    }
}
