using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
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

        private static readonly JsonSerializerOptions _defaultOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

            PropertyNameCaseInsensitive = true,
        };

        private static readonly JsonSerializerOptions _camelCaseOptions = new()
        {
            Encoder = _defaultOptions.Encoder,
            PropertyNameCaseInsensitive = _defaultOptions.PropertyNameCaseInsensitive,

            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private static readonly JsonSerializerOptions _snakeCaseOptions = new()
        {
            Encoder = _defaultOptions.Encoder,
            PropertyNameCaseInsensitive = _defaultOptions.PropertyNameCaseInsensitive,

            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };

        internal static void SetDefaultSerializerOptions(Action<JsonSerializerOptions> configure)
        {
            configure?.Invoke(_defaultOptions);
        }

        public static string Serialize(object value, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Serialize(value, options ?? _defaultOptions);
        }

        public static T Deserialize<T>(string json, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }

        public static string SerializeSnakeCase(object value)
        {
            return Serialize(value, _snakeCaseOptions);
        }

        public static T DeserializeSnakeCase<T>(string json)
        {
            return Deserialize<T>(json, _snakeCaseOptions);
        }

        public static string SerializeCamelCase(object value)
        {
            return Serialize(value, _camelCaseOptions);
        }

        public static T DeserializeCamelCase<T>(string json)
        {
            return Deserialize<T>(json, _camelCaseOptions);
        }
    }
}
