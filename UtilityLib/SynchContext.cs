using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class SynchContext
    {

        public static SynchronizationContext SynchronizationContextStatic => SynchronizationContext.Current;
    }
}
