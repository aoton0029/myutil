﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Test1
{
    public interface INavigablePage
    {
        void OnNavigated(object sharedData, object tempData);
    }
}
