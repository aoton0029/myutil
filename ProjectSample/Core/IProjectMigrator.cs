using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Core
{
    public interface IProjectMigrator
    {
        string FromVersion { get; }
        string ToVersion { get; }

        Project Migrate(Project oldProject);
    }

}
