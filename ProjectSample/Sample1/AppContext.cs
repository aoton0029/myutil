using ProjectSample.Core;
using ProjectSample.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample
{
    public class AppContext
    {
        public AppSetting Setting { get; set; } = new();
        public ProjectContext ProjectContext { get; set; } = new();
        public ProjectService ProjectService { get; set; }

        public AppContext()
        {

        }
    }

}
