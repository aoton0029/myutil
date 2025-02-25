using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SampleTcpServer.Simulators
{
    interface IDeviceProtocol
    {
        string Encode(string command); // 送信データをエンコード
        string Decode(string response); // 受信データをデコード
    }

    class PowerSupplyProtocol : IDeviceProtocol
    {
        public string Encode(string command)
        {
            return $"PS:{command}"; // 例: "PS:SET_VOLTAGE 12.5"
        }

        public string Decode(string response)
        {
            return response.StartsWith("PS:") ? response.Substring(3) : "ERROR: INVALID RESPONSE";
        }
    }

    class MultimeterProtocol : IDeviceProtocol
    {
        public string Encode(string command)
        {
            return JsonSerializer.Serialize(new { cmd = command });
        }

        public string Decode(string response)
        {
            try
            {
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(response);
                return result != null && result.ContainsKey("result") ? result["result"] : "ERROR: INVALID RESPONSE";
            }
            catch
            {
                return "ERROR: JSON PARSE FAILED";
            }
        }
    }

    class TemperatureSensorProtocol : IDeviceProtocol
    {
        public string Encode(string command)
        {
            return command switch
            {
                "GET_TEMPERATURE" => "\x01", // 1バイトでコマンド送信
                "GET_HUMIDITY" => "\x02",
                _ => "\xFF" // エラー
            };
        }

        public string Decode(string response)
        {
            return response switch
            {
                "\x01" => "TEMPERATURE 25°C",
                "\x02" => "HUMIDITY 60%",
                _ => "ERROR: INVALID RESPONSE"
            };
        }
    }
}
