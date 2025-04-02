using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class ProjectService
    {
        private readonly JsonConverter _converter;

        public ProjectService(JsonConverter converter)
        {
            _converter = converter;
        }

        public Project Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var project = _converter.Deserialize<Project>(json);
            // バージョンチェックやマイグレーション
            return MigrateIfNeeded(project);
        }

        public void Save(Project project, string filePath)
        {
            project.Version++; // 保存ごとにバージョンアップ（任意）
            var json = _converter.Serialize(project);
            File.WriteAllText(filePath, json);
        }

        private Project MigrateIfNeeded(Project project)
        {
            // 古いバージョンのプロジェクトが読み込まれたときの処理
            if (project.Version < CurrentVersion)
            {
                // マイグレーション処理（簡略化例）
                project.Version = CurrentVersion;
            }

            return project;
        }

        private const int CurrentVersion = 2;
    }

}
