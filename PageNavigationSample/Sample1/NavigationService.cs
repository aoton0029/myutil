using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PageNavigationSample.Sample1
{

    public class NavigationService
    {
        private readonly Stack<UserControl> _historyStack = new Stack<UserControl>();

        public event Action<UserControl> OnNavigate;

        public void Navigate(PageKey key, object data)
        {
            Type pageType = PageRegistry.GetPageType(key);
            UserControl newControl = (UserControl)Activator.CreateInstance(pageType);

            if (newControl is IPage page)
            {
                page.UpdateData(data);
                page.IShown();
            }

            _historyStack.Push(newControl);
            OnNavigate?.Invoke(newControl);
        }

        public void GoBack()
        {
            if (_historyStack.Count > 1)
            {
                _historyStack.Pop();
                var previousControl = _historyStack.Peek();
                OnNavigate?.Invoke(previousControl);

                if (previousControl is IPage page)
                {
                    page.IShown();
                }
            }
        }

        public List<string> GetBreadcrumbs()
        {
            return _historyStack.Select(c => c.GetType().Name).ToList();
        }

        public void ClearHistory()
        {
            _historyStack.Clear();
        }
    }
}
