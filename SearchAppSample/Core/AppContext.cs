using SearchAppSample.Models.SearchConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SearchAppSample.Core
{
    public class AppContext
    {
        private string _filePath;

        public AppSettings AppSettings { get; set; } = new AppSettings();

        public History<SrchCondKouban> SearchResultKouban { get; set; }
        public History<SrchCondBN> SearchResultBN { get; set; }
        public History<SrchCondChuban> SearchResultChuban { get; set; }
        public History<SrchCondKatamei> SearchResultKatamei { get; set; }
        public History<SrchCondPN> SearchResultPN { get; set; }
        public History<SrchCondSN> SearchResultSN { get; set; }
        public UserRank UserRank { get; set; } = UserRank.Standard;

        // JSONファイルに保存
        public void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true // 読みやすいフォーマットで出力
            };

            // シリアライズしてファイルに書き込む
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(_filePath, json);
        }

        // JSONファイルから読込
        public AppContext Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            // ファイルを読み込んでデシリアライズ
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<AppContext>(json);
        }
    }

    public enum UserRank
    {
        Guest,
        Standard,
        Premium,
        Admin
    }
}
