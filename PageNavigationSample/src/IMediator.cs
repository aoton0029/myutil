using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample
{
    public interface IMediator
    {
        void Register(string eventCode, Action<object> handler);
        void Unregister(string eventCode, Action<object> handler);
        void Notify(string eventCode, object data);
    }
}
