using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Sample2
{
    public class BaseController
    {
        protected NavigationService _navigationService;

        public BaseController()
        {
            _navigationService = NavigationService.Instance;
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public void GoForward()
        {
            _navigationService.GoForward();
        }

        public virtual void NavigateToHome()
        {
            // 派生クラスで実装される
        }

        public virtual void Cancel()
        {
            // 派生クラスで実装される
        }
    }
}
