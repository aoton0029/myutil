using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class ImageCache
    {
        private readonly string folderPath;
        private readonly ConcurrentDictionary<string, Task<Image>> cache;

        public ImageCache(string folderPath)
        {
            this.folderPath = folderPath;
            this.cache = new ConcurrentDictionary<string, Task<Image>>();
            PreloadImagesAsync();
        }

        // 非同期でフォルダ内のすべての画像をキャッシュ
        private async void PreloadImagesAsync()
        {
            var files = Directory.GetFiles(folderPath, "*.jpg")
                .Concat(Directory.GetFiles(folderPath, "*.png"))
                .ToList();

            var tasks = files.Select(file => LoadImageAsync(file));

            foreach (var task in tasks)
            {
                try
                {
                    await task; // キャッシュに登録中のタスクが完了するのを待つ
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image: {ex.Message}");
                }
            }
        }

        // 非同期で画像を読み込む（要求があれば優先的に）
        public async Task<Image> GetImageAsync(string imageName)
        {
            string filePath = Path.Combine(folderPath, imageName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Image not found.", filePath);
            }

            return await cache.GetOrAdd(filePath, path => LoadImageAsync(path));
        }

        // 非同期で画像を読み込むタスクを作成
        private async Task<Image> LoadImageAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return Image.FromStream(stream);
                }
            });
        }
    }
}
