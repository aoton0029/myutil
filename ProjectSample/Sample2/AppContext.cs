using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    class AppContext
    {
        public AppSetting AppSetting { get; set; }

        public ProjectContext ProjectContext { get; set; }

        public ProjectService ProjectService { get; set; }

        public EventBus EventBus { get; set; }

        public AppContext()
        {
            EventBus = new EventBus();
            ProjectContext = new ProjectContext();
            ProjectService = new ProjectService(ProjectContext);
        }
    }
}
