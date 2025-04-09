using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectSample.Core
{
    public class Project
    {
        public string Version { get; set; } = "1.0";
        public string Name { get; set; }
        public List<ProjectItem> Items { get; set; } = new();

        public string ToJson() =>
            JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

        public static Project FromJson(string json) =>
            JsonSerializer.Deserialize<Project>(json);
    }

}
