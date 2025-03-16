using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace UtilityLib.Images
{
    public class ImageCache
    {
        private readonly ConcurrentDictionary<string, (Image Image, DateTime Expiration)> _cache = new();
        private readonly TimeSpan? _defaultCacheDuration;
        private readonly Channel<string> _loadQueue = Channel.CreateUnbounded<string>();
        private readonly string _folderPath;
        private readonly CancellationTokenSource _cts = new();

        public ImageCache(string folderPath, TimeSpan? cacheDuration = null)
        {
            _folderPath = folderPath;
            _defaultCacheDuration = cacheDuration;
            StartBackgroundLoading();
        }

        private void StartBackgroundLoading()
        {
            Task.Run(async () =>
            {
                await foreach (var file in _loadQueue.Reader.ReadAllAsync(_cts.Token))
                {
                    await LoadImageAsync(file);
                }
            });

            Task.Run(() => PreloadImages());
        }

        private void PreloadImages()
        {
            foreach (var file in Directory.GetFiles(_folderPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                if (IsImageFile(file))
                {
                    _loadQueue.Writer.TryWrite(file);
                }
            }
        }

        private static bool IsImageFile(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif";
        }

        private async Task LoadImageAsync(string filePath)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var img = Image.FromStream(stream);
                _cache[filePath] = (img, _defaultCacheDuration.HasValue ? DateTime.UtcNow + _defaultCacheDuration.Value : DateTime.MaxValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {filePath} - {ex.Message}");
            }
        }

        public async Task<Image?> GetImageAsync(string filePath)
        {
            if (_cache.TryGetValue(filePath, out var cached) && cached.Expiration > DateTime.UtcNow)
            {
                return cached.Image;
            }

            await LoadImageAsync(filePath);
            return _cache.TryGetValue(filePath, out var newCache) ? newCache.Image : null;
        }

        public void Dispose()
        {
            _cts.Cancel();
            foreach (var (_, (img, _)) in _cache)
            {
                img.Dispose();
            }
            _cache.Clear();
        }
    }
}
