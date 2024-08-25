using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Sample1
{
    public abstract class PageBase : UserControl, IPage
    {
        protected MainController _controller;

        // デザイナーで使用するための引数なしコンストラクタを追加
        protected PageBase() : this(null) { }

        // MainControllerを受け取るコンストラクタ
        protected PageBase(MainController controller)
        {
            _controller = controller;
        }

        public abstract void UpdateData(object data);
        public abstract void IShown();

        public void NavigateTo(PageKey key)
        {
            _controller?.NavigateTo(key);
        }

        public void GoBack()
        {
            _controller?.GoBack();
        }

        public void Cancel()
        {
            _controller?.Cancel();
        }

        public void GoHome()
        {
            _controller?.GoHome();
        }
    }
}
