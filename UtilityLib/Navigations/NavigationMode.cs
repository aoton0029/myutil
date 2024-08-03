using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Navigations
{
    public enum NavigationMode
    {
        //
        // 概要:
        //     Navigation is to a new instance of a page (not going forward or backward in the
        //     stack).
        New,
        //
        // 概要:
        //     Navigation is going backward in the stack.
        Back,
        //
        // 概要:
        //     Navigation is going forward in the stack.
        Forward,
        //
        // 概要:
        //     Navigation is to the current page (perhaps with different data).
        Refresh
    }
}
