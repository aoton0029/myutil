using System;
using System.Collections;
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

        //public static void JsonDeserialize()
        //{
        //    string jsonString = @"
        //    {
        //      ""object"": ""page"",
        //      ""id"": ""aa3a810b-c453-45c1-bc44-00c49f984a0f"",
        //      ""created_time"": ""2022-05-02T05:45:00.000Z"",
        //      ""last_edited_time"": ""2024-11-21T11:01:00.000Z"",
        //      ""created_by"": {
        //        ""object"": ""user"",
        //        ""id"": ""c6ea8f84-b4fd-4c68-a4e4-e7f1002d4004""
        //      },
        //      ""last_edited_by"": {
        //        ""object"": ""user"",
        //        ""id"": ""c6ea8f84-b4fd-4c68-a4e4-e7f1002d4004""
        //      },
        //      ""cover"": null,
        //      ""icon"": {
        //        ""type"": ""emoji"",
        //        ""emoji"": ""📚""
        //      },
        //      ""parent"": {
        //        ""type"": ""workspace"",
        //        ""workspace"": true
        //      },
        //      ""archived"": false,
        //      ""in_trash"": false,
        //      ""properties"": {
        //        ""title"": {
        //          ""id"": ""title"",
        //          ""type"": ""title"",
        //          ""title"": [
        //            {
        //              ""type"": ""text"",
        //              ""text"": {
        //                ""content"": ""読書記録"",
        //                ""link"": null
        //              },
        //              ""annotations"": {
        //                ""bold"": false,
        //                ""italic"": false,
        //                ""strikethrough"": false,
        //                ""underline"": false,
        //                ""code"": false,
        //                ""color"": ""default""
        //              },
        //              ""plain_text"": ""読書記録"",
        //              ""href"": null
        //            }
        //          ]
        //        }
        //      },
        //      ""url"": ""https://www.notion.so/aa3a810bc45345c1bc4400c49f984a0f"",
        //      ""public_url"": null,
        //      ""request_id"": ""c9c3ca44-72ee-4449-960c-3b552f030b87""
        //    }";

        //    // JSON文字列を解析
        //    JavaScriptSerializer serializer = new JavaScriptSerializer();
        //    var jsonObject = serializer.Deserialize<Dictionary<string, object>>(jsonString);

        //    // 全プロパティを列挙
        //    PrintJson(jsonObject, "");

        //    void PrintJson(object obj, string indent)
        //    {
        //        if (obj is Dictionary<string, object> dict)
        //        {
        //            foreach (var key in dict.Keys)
        //            {
        //                Console.WriteLine($"{indent}{key}:");
        //                PrintJson(dict[key], indent + "  ");
        //            }
        //        }
        //        else if (obj is ArrayList list)
        //        {
        //            foreach (var item in list)
        //            {
        //                PrintJson(item, indent + "  ");
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine($"{indent}{obj}");
        //        }
        //    }
        //}

        public static void Deserialize()
        {
            // サンプルJSON
            string json = @"
            {
                ""name"": ""John"",
                ""age"": 30,
                ""isStudent"": false,
                ""skills"": [""C#"", ""JavaScript"", ""SQL""],
                ""address"": {
                    ""city"": ""Tokyo"",
                    ""zipCode"": ""123-4567""
                }
            }";

            // JsonDocumentを使用して解析
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement root = document.RootElement;

                // 値の取得
                string name = root.GetProperty("name").GetString();
                int age = root.GetProperty("age").GetInt32();
                bool isStudent = root.GetProperty("isStudent").GetBoolean();

                Console.WriteLine($"Name: {name}");
                Console.WriteLine($"Age: {age}");
                Console.WriteLine($"Is Student: {isStudent}");

                // 配列の取得
                JsonElement skills = root.GetProperty("skills");
                Console.WriteLine("Skills:");
                foreach (JsonElement skill in skills.EnumerateArray())
                {
                    Console.WriteLine($"- {skill.GetString()}");
                }

                // ネストされたオブジェクトの取得
                JsonElement address = root.GetProperty("address");
                string city = address.GetProperty("city").GetString();
                string zipCode = address.GetProperty("zipCode").GetString();

                Console.WriteLine($"City: {city}");
                Console.WriteLine($"Zip Code: {zipCode}");
            }
        }
    }
}
