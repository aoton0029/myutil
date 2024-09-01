using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tcp.Simple
{
    public class CommResult
    {
        public string Data { get; set; }
        public bool IsSuccess { get; set; }
        public Exception exception { get; set; }
        public string ErrorMessage { get; set; }

        public CommResult(string data, bool isSuccess, string errorMessage = null)
        {
            Data = data;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static CommResult Success(string data)
        {
            return new CommResult(data, true);
        }

        public static CommResult Exeption(string errMsg)
        {
            return new CommResult("", false, errMsg);
        }
    }
}
