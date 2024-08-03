using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Navigations.WinUI
{
    internal interface INavigationEventArgs
    {
        object Content { get; }

        NavigationMode NavigationMode { get; }

        NavigationTransitionInfo NavigationTransitionInfo { get; }

        object Parameter { get; }

        Type SourcePageType { get; }

        Uri Uri { get; set; }
    }
}
