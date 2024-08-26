using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Sample1
{
    public abstract class BasePage : UserControl, IPage
    {
        public MainController Controller { get; set; }

        public BasePage(MainController controller)
        {
            Controller = controller;
        }

        public abstract void UpdateData(object data);
        public abstract void IShown();

        // ページからナビゲーションを実行するためのメソッド
        protected void NavigateTo(PageKey key)
        {
            Controller.NavigateTo(key);
        }

        protected void GoBack()
        {
            Controller.GoBack();
        }

        protected void Cancel()
        {
            Controller.Cancel();
        }

        protected void GoHome()
        {
            Controller.GoHome();
        }
    }
}
