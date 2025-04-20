using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectSample.Core
{
    public class AppSetting
    {
        public string LastOpenedProjectPath { get; set; }

        public static AppSetting Load(string path) =>
            File.Exists(path)
                ? JsonSerializer.Deserialize<AppSetting>(File.ReadAllText(path))
                : new AppSetting();

        public void Save(string path) =>
            File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

}
