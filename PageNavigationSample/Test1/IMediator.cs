using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Test1
{
    interface IMediator
    {

    }

    public class DataMediator
    {
        private readonly Dictionary<string, object> _sharedData = new();

        public void Register(string key, ObservableObject sharedData) => _sharedData[key] = sharedData;

        public ObservableObject GetSharedData(string key) => _sharedData[key] as ObservableObject;
    }
}
