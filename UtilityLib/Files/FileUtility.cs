using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Files
{
    public class FileUtility
    {
        public static void CreateDirectory(string filePath)
        {
            var directory = System.IO.Path.GetDirectoryName(filePath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
        }

        public static System.Drawing.Image LoadImage(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return null;
            }

            using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                return System.Drawing.Image.FromStream(fs);
            }
        }
    }
}
