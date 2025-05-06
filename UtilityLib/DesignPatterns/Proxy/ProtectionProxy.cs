using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Proxy
{
    // 共有インターフェース
    public interface IDataAccess
    {
        void AccessData();
    }

    // 実際のデータアクセスクラス
    public class RealDataAccess : IDataAccess
    {
        public void AccessData()
        {
            Console.WriteLine("Accessing sensitive data...");
        }
    }

    // プロキシクラス（アクセス制御）
    public class ProtectionProxy : IDataAccess
    {
        private RealDataAccess _realDataAccess;
        private string _userRole;

        public ProtectionProxy(string userRole)
        {
            _userRole = userRole;
        }

        public void AccessData()
        {
            if (_userRole == "Admin")
            {
                if (_realDataAccess == null)
                    _realDataAccess = new RealDataAccess();

                _realDataAccess.AccessData();
            }
            else
            {
                Console.WriteLine("Access Denied: You do not have permission.");
            }
        }
    }

   
    
}
