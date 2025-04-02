using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class JsonConverter
    {
        public string Serialize<T>(T obj) => System.Text.Json.JsonSerializer.Serialize(obj);
        public T Deserialize<T>(string json) => System.Text.Json.JsonSerializer.Deserialize<T>(json)!;
    }

}
