using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PageNavigationSample.Sample1
{
    public partial class UcPage : UserControl
    {
        protected readonly NavigationService _navigationService;

        public UcPage()
        {
            _navigationService = NavigationService.Instance;
        }

        // 次の画面に進む機能
        protected void NavigateToNext(UserControl nextControl)
        {
            _navigationService.Navigate(nextControl, NavigationService.NavigationMode.UseExistingInstance);
        }

        // 前の画面に戻る機能
        protected void GoBack()
        {
            _navigationService.GoBack();
        }

        // ホーム画面に戻る機能
        protected void NavigateToHome(UserControl homeControl)
        {
            _navigationService.Navigate(homeControl, NavigationService.NavigationMode.UseExistingInstance);
        }

        // 特定のページに遷移する機能
        protected void NavigateToSpecificPage(UserControl specificControl)
        {
            _navigationService.Navigate(specificControl, NavigationService.NavigationMode.CreateNewInstance);
        }
    }
}
