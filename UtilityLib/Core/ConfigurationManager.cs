using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Core
{
    using System;
    using System.IO;
    using System.Text.Json;

    namespace SearachAppSample.Core
    {
        public class ConfigurationManager<T> where T : class, new()
        {
            private readonly string _configPath;
            private T _currentConfig;

            public ConfigurationManager(string fileName = "config.json")
            {
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SearachAppSample");

                Directory.CreateDirectory(appDataPath);
                _configPath = Path.Combine(appDataPath, fileName);
            }

            public T LoadConfiguration()
            {
                if (_currentConfig != null)
                    return _currentConfig;

                if (!File.Exists(_configPath))
                {
                    _currentConfig = new T();
                    SaveConfiguration(_currentConfig);
                    return _currentConfig;
                }

                try
                {
                    string json = File.ReadAllText(_configPath);
                    _currentConfig = JsonSerializer.Deserialize<T>(json) ?? new T();
                    return _currentConfig;
                }
                catch
                {
                    _currentConfig = new T();
                    SaveConfiguration(_currentConfig);
                    return _currentConfig;
                }
            }

            public void SaveConfiguration(T config)
            {
                _currentConfig = config;
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(_configPath, json);
            }
        }
    }

}
