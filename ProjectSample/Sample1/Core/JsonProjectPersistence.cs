using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectSample.Core
{
    public class JsonProjectPersistence : IProjectPersistence
    {
        public Project Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Project>(json);
        }

        public void Save(Project project, string filePath)
        {
            var json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
    }

}
