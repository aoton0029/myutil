using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Interfaces
{
    internal class interfaces
    {
        public interface IFileHandler
        {
            void Write(string path, string content);
            string Read(string path);
            void Delete(string path);
        }

        public interface IObjectSerializer
        {
            string? Serialize<T>(T value);

            T Deserialize<T>(string value);
        }


        public interface ISettingsStorageHelper<in TKey> where TKey : notnull
        {
            bool TryRead<TValue>(TKey key, out TValue? value);

            void Save<TValue>(TKey key, TValue value);

            bool TryDelete(TKey key);

            void Clear();
        }

        public class SystemSerializer : IObjectSerializer
        {
            public T Deserialize<T>(string value)
            {
                if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                throw new NotSupportedException("This serializer can only handle primitive types and strings. Please implement your own IObjectSerializer for more complex scenarios.");
            }

            public string? Serialize<T>(T value)
            {
                return value?.ToString();
            }
        }
    }
}
