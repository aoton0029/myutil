using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    class ProjectContext
    {
        public DateTime CreatedAt { get; set; }

        public Project CurrentProject { get; set; }

        public string Version { get; set; }

        public DateTime LastModifiedTime { get; set; }

        public bool IsModified { get; set; }
    }
}
