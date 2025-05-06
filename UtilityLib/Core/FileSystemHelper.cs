using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Core
{
    public static class FileSystemHelper
    {
        public static string GetAppDataPath(string folderName = null)
        {
            string basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SearachAppSample");

            if (!string.IsNullOrEmpty(folderName))
            {
                basePath = Path.Combine(basePath, folderName);
            }

            Directory.CreateDirectory(basePath);
            return basePath;
        }

        public static bool SafeDeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool SafeCopyFile(string sourcePath, string destPath, bool overwrite = true)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destPath, overwrite);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public static async Task WriteAllTextAsync(string filePath, string content, CancellationToken cancellationToken = default)
        {
            using var stream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true);

            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
        }
    }
}
