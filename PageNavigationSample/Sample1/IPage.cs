using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Sample1
{
    public interface IPage
    {
        void UpdateData(object data);

        void IShown();
    }
}
