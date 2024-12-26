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

        public interface ICache<T>
        {
            void Add(string key, T value);
            T Get(string key);
            bool Contains(string key);
            void Remove(string key);
        }





    }
}
