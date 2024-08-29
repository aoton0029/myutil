using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Sample2
{
    public class HistoryItem
    {
        public string PageName { get; set; }
        public object NavigationParameter { get; set; }
        public UserControl Page { get; set; }

        public HistoryItem(string pageName, object parameter, UserControl page)
        {
            PageName = pageName;
            NavigationParameter = parameter;
            Page = page;
        }
    }
}
