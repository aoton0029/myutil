using ProjectSample.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectSample.Services
{
    public class ProjectService
    {
        private readonly IProjectPersistence _persistence;
        private readonly ProjectMigrationEngine _migrationEngine;
        private readonly ProjectContext _context;

        public ProjectService(ProjectContext context, IProjectPersistence persistence, ProjectMigrationEngine migrationEngine)
        {
            _context = context;
            _persistence = persistence;
            _migrationEngine = migrationEngine;
        }

        public void Save(string filePath)
        {
            var json = JsonSerializer.Serialize(_context.Current, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                SafeFileWriter.WriteAtomic(filePath, json);
                _context.MarkSaved();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存に失敗しました: {ex.Message}", "保存エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool TryLoad(string filePath)
        {
            try
            {
                var project = _persistence.Load(filePath);
                project = _migrationEngine.MigrateToLatest(project);
                _context.Load(project, filePath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"読み込みに失敗しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }


}
