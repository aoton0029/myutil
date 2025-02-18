using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormSample
{
    public partial class Form4 : Form
    {
        private readonly ImageLoader imageLoader;
        private readonly Dictionary<string, PictureBox> pictureBoxes = new();

        public Form4()
        {
            InitializeComponent();

            string imageDirectory = @"D:\notoa\Pictures\画像";
            imageLoader = new ImageLoader(imageDirectory);

            // 画像が読み込まれたときのイベントを購読
            imageLoader.OnImageLoaded += ImageLoader_OnImageLoaded;
        }

        private void ImageLoader_OnImageLoaded(object sender, ImageLoadedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                if (!pictureBoxes.ContainsKey(e.Key))
                {
                    var pb = new PictureBox
                    {
                        Name = e.Key,
                        Size = new Size(150, 150),
                        Location = new Point(10, 10 + pictureBoxes.Count * 160),
                        BorderStyle = BorderStyle.FixedSingle,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Image = e.Image
                    };
                    pictureBoxes[e.Key] = pb;
                    flowLayoutPanel1.Controls.Add(pb);
                }
            }));
        }

        public class ImageLoader
        {
            private readonly ConcurrentDictionary<string, Image> imageCache = new();
            private readonly string imageDirectory;
            private readonly CancellationTokenSource cts = new();
            private readonly Task producerTask;

            // 画像が追加されたときのイベント
            public event EventHandler<ImageLoadedEventArgs> OnImageLoaded;

            public ImageLoader(string directory)
            {
                if (!Directory.Exists(directory))
                {
                    throw new DirectoryNotFoundException($"Directory not found: {directory}");
                }

                imageDirectory = directory;
                producerTask = Task.Run(() => LoadImagesAsync(cts.Token));
            }

            private async Task LoadImagesAsync(CancellationToken token)
            {
                var imageFiles = Directory.GetFiles(imageDirectory, "*.jpg");

                foreach (var filePath in imageFiles)
                {
                    if (token.IsCancellationRequested) break;

                    string key = Path.GetFileName(filePath);
                    try
                    {
                        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        var img = Image.FromStream(fs);

                        if (imageCache.TryAdd(key, img))
                        {
                            // イベントを発火 (画像が追加されたことを通知)
                            OnImageLoaded?.Invoke(this, new ImageLoadedEventArgs(key, img));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading {key}: {ex.Message}");
                    }

                    await Task.Delay(500); // 画像のロード間隔を設定
                }
            }

            public Image GetImage(string key)
            {
                return imageCache.TryGetValue(key, out var img) ? img : null;
            }

            public void Dispose()
            {
                cts.Cancel();
                producerTask.Wait();
            }
        }

        // 画像が読み込まれたことを通知するためのイベント引数クラス
        public class ImageLoadedEventArgs : EventArgs
        {
            public string Key { get; }
            public Image Image { get; }

            public ImageLoadedEventArgs(string key, Image image)
            {
                Key = key;
                Image = image;
            }
        }
    }
}
