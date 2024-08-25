using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PageNavigationSample.Sample1
{
    public enum PageKey
    {
        Home,
        Child1,
        Child2,
        Settings
    }

    public class NavigationService
    {
        public enum NavigationMode
        {
            UseExistingInstance, // 既存のインスタンスを使用して遷移
            CreateNewInstance    // 新しいインスタンスを作成して遷移
        }

        private static NavigationService _instance;
        private readonly Stack<UserControl> _historyStack = new Stack<UserControl>();
        private readonly Dictionary<PageKey, Type> _pageRegistry = new Dictionary<PageKey, Type>();
        private readonly NavigationMode _defaultMode;

        private NavigationService(NavigationMode defaultMode)
        {
            _defaultMode = defaultMode;
        }

        public static NavigationService GetInstance(NavigationMode defaultMode)
        {
            return _instance ??= new NavigationService(defaultMode);
        }

        public event Action<UserControl> OnNavigate;

        public void RegisterPage(PageKey key, Type pageType)
        {
            if (!_pageRegistry.ContainsKey(key))
            {
                _pageRegistry.Add(key, pageType);
            }
        }

        public void Navigate(PageKey key, object data)
        {
            if (_pageRegistry.ContainsKey(key))
            {
                Type pageType = _pageRegistry[key];

                UserControl newControl;
                if (_defaultMode == NavigationMode.UseExistingInstance)
                {
                    newControl = _historyStack.FirstOrDefault(c => c.GetType() == pageType) ?? (UserControl)Activator.CreateInstance(pageType);
                }
                else
                {
                    newControl = (UserControl)Activator.CreateInstance(pageType);
                }

                if (newControl is IPage page)
                {
                    page.UpdateData(data);
                    page.IShown();
                }

                _historyStack.Push(newControl);
                OnNavigate?.Invoke(newControl);
            }
            else
            {
                throw new ArgumentException("指定されたページキーは登録されていません。", nameof(key));
            }
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
