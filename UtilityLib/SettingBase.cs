using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class SettingBase
    {
        public string FilePath { get; set; }

        public void Save<T>(T obj)
        { 
            Files.JsonUtility.SaveJsonFile(FilePath, obj);
        }

        public T Load<T>()
        {
            return Files.JsonUtility.LoadJsonFile<T>(FilePath);
        }
    }
}
