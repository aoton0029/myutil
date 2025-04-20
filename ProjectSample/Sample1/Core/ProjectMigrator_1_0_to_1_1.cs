using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Core
{
    public class ProjectMigrator_1_0_to_1_1 : IProjectMigrator
    {
        public string FromVersion => "1.0";
        public string ToVersion => "1.1";

        public Project Migrate(Project oldProject)
        {
            // 例: ProjectItem に Description フィールド追加など
            //foreach (var item in oldProject.Items)
            //{
            //    if (item is ProjectItemWithDescription descItem)
            //        descItem.Description ??= "default";
            //}

            oldProject.Version = ToVersion;
            return oldProject;
        }
    }

}
