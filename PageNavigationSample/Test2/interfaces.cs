using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Test2
{
    public interface INextPageDecider
    {
        Type? DecideNextPage();
    }

    public interface IShown
    {
        void OnShown();
    }
}
