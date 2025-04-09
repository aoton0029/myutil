using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Core
{
    public class ProjectMigrationEngine
    {
        private readonly List<IProjectMigrator> _migrators;

        public ProjectMigrationEngine(IEnumerable<IProjectMigrator> migrators)
        {
            _migrators = migrators.OrderBy(m => m.FromVersion).ToList();
        }

        public Project MigrateToLatest(Project project)
        {
            string current = project.Version;
            while (true)
            {
                var migrator = _migrators.FirstOrDefault(m => m.FromVersion == current);
                if (migrator == null) break;

                project = migrator.Migrate(project);
                current = project.Version;
            }

            return project;
        }
    }

}
