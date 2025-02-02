using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Proxy
{
    // インターフェース
    public interface IDataFetcher
    {
        string GetData(int id);
    }

    // 実際のデータ取得クラス
    public class RealDataFetcher : IDataFetcher
    {
        public string GetData(int id)
        {
            Console.WriteLine($"Fetching data for ID: {id} from database...");
            return $"Data for {id}";
        }
    }

    // キャッシュプロキシ
    public class CacheProxy : IDataFetcher
    {
        private RealDataFetcher _realDataFetcher;
        private Dictionary<int, string> _cache = new Dictionary<int, string>();

        public CacheProxy()
        {
            _realDataFetcher = new RealDataFetcher();
        }

        public string GetData(int id)
        {
            if (_cache.ContainsKey(id))
            {
                Console.WriteLine($"Returning cached data for ID: {id}");
                return _cache[id];
            }

            string data = _realDataFetcher.GetData(id);
            _cache[id] = data;
            return data;
        }
    }

    // 使用例
    class Program
    {
        static void Main()
        {
            IDataFetcher dataFetcher = new CacheProxy();

            Console.WriteLine(dataFetcher.GetData(1)); // DBアクセス
            Console.WriteLine(dataFetcher.GetData(1)); // キャッシュから取得
            Console.WriteLine(dataFetcher.GetData(2)); // DBアクセス
        }
    }
}
