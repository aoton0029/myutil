﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Proxy
{
    public interface IImage
    {
        void Display();
    }

    // 実際の画像クラス（Real Subject）
    public class RealImage : IImage
    {
        private string _filename;

        public RealImage(string filename)
        {
            _filename = filename;
            LoadImageFromDisk(); // コンストラクタで画像をロード
        }

        private void LoadImageFromDisk()
        {
            Console.WriteLine($"Loading image: {_filename}");
        }

        public void Display()
        {
            Console.WriteLine($"Displaying image: {_filename}");
        }
    }

    // プロキシクラス
    public class ProxyImage : IImage
    {
        private RealImage _realImage;
        private string _filename;

        public ProxyImage(string filename)
        {
            _filename = filename;
        }

        public void Display()
        {
            // 初回アクセス時のみ実際の画像をロード
            if (_realImage == null)
            {
                _realImage = new RealImage(_filename);
            }
            _realImage.Display();
        }
    }

}
