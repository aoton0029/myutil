using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    class AppSetting
    {
        public string LastOpenedProjectPath { get; set; }

        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(path, json);
        }

        public static AppSetting Load(string path)
        {
            if (!File.Exists(path)) return new AppSetting();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSetting>(json) ?? new AppSetting();
        }
    }
}
