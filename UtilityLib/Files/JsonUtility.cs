using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace UtilityLib.Files
{
    public class JsonUtility
    {
        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        
        // jsonファイルを読み込む
        public static T LoadJsonFile<T>(string path)
        {
            string json = System.IO.File.ReadAllText(path);
            return Deserialize<T>(json);
        }

        // jsonファイルに書き込む
        public static void SaveJsonFile<T>(string path, T obj)
        {
            string json = Serialize(obj);
            FileUtility.CreateDirectory(path);
            System.IO.File.WriteAllText(path, json);
        }
    }
}
