using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class Interop
    {
        public struct EventArguments<TSender, TEventArgs>
        {
            public TSender Sender { get; set; }

            public TEventArgs EventArgs { get; set; }
        }


    }
}
