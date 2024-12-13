using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UtilityLib.Models
{
    

    public class ExchangeRate
    {
        public string CurrencyCode { get; set; } // 通貨コード (例: "USD", "EUR")
        public decimal RateToJPY { get; set; } // 日本円への変換レート
        public DateTime RetrievedAt { get; set; } // 取得日時
    }

    public class Currency
    {
        private Dictionary<string, ExchangeRate> ExchangeRates { get; set; }

        public Currency()
        {
            ExchangeRates = new Dictionary<string, ExchangeRate>();
        }

        // 為替レートを追加または更新
        public void UpdateExchangeRate(string currencyCode, decimal rateToJPY)
        {
            var exchangeRate = new ExchangeRate
            {
                CurrencyCode = currencyCode,
                RateToJPY = rateToJPY,
                RetrievedAt = DateTime.UtcNow
            };

            ExchangeRates[currencyCode] = exchangeRate;
        }

        // 指定通貨を日本円に変換
        public decimal ConvertToJPY(string currencyCode, decimal amount)
        {
            if (!ExchangeRates.ContainsKey(currencyCode))
            {
                throw new ArgumentException($"Exchange rate for currency code '{currencyCode}' is not available.");
            }

            var rate = ExchangeRates[currencyCode].RateToJPY;
            return amount * rate;
        }

        // 為替レートのJSON表現を取得
        public string GetExchangeRatesAsJson()
        {
            return JsonSerializer.Serialize(ExchangeRates.Values, new JsonSerializerOptions
            {
                WriteIndented = true // JSONを整形して出力
            });
        }
    }
}
