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
        void NavigateTo(PageKey key);
        void GoBack();
        void Cancel();   // キャンセル機能
        void GoHome();   // ホームに戻る機能
    }

    interface IController 
    {
        void Navigate(string action, object parameter);
    }

}
