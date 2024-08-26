using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Sample1
{
    public enum NavigationMode
    {
        UseExistingInstance, // 既存のインスタンスを使用して遷移
        CreateNewInstance    // 新しいインスタンスを作成して遷移
    }

    public class MainController
    {
        private readonly Stack<UserControl> _historyStack = new Stack<UserControl>();
        private readonly Panel _mainPanel;
        private object _sharedData;
        private readonly PageKey _homePageKey;
        private readonly NavigationMode _navigationMode;

        public MainController(Panel mainPanel, PageKey homePageKey, NavigationMode navigationMode)
        {
            _mainPanel = mainPanel;
            _homePageKey = homePageKey;
            _navigationMode = navigationMode;
            _sharedData = null;
        }

        public void NavigateTo(PageKey key)
        {
            Type pageType = PageRegistry.GetPageType(key);
            UserControl newControl;

            if (_navigationMode == NavigationMode.UseExistingInstance)
            {
                newControl = _historyStack.FirstOrDefault(c => c.GetType() == pageType) ?? (UserControl)Activator.CreateInstance(pageType);
            }
            else
            {
                newControl = (UserControl)Activator.CreateInstance(pageType);
            }

            if (newControl is IPage page)
            {
                page.UpdateData(_sharedData);
                page.IShown();
            }

            _historyStack.Push(newControl);
            UpdateUI(newControl);
        }

        public void GoBack()
        {
            if (_historyStack.Count > 1)
            {
                _historyStack.Pop();
                var previousControl = _historyStack.Peek();
                UpdateUI(previousControl);

                if (previousControl is IPage page)
                {
                    page.IShown();
                }
            }
        }

        public void Cancel()
        {
            GoBack();
        }

        public void GoHome()
        {
            _historyStack.Clear();
            NavigateTo(_homePageKey);
        }

        private void UpdateUI(UserControl control)
        {
            _mainPanel.Controls.Clear();
            control.Dock = DockStyle.Fill;
            _mainPanel.Controls.Add(control);
            UpdateBreadcrumbs();
        }

        private void UpdateBreadcrumbs()
        {
            var breadcrumbs = _historyStack.Select(c => c.GetType().Name).ToList();
            // パンくずリストの更新処理をここに追加します
        }

        public void UpdateSharedData(object newValue)
        {
            _sharedData = newValue;
        }

        public T GetSharedData<T>()
        {
            return (T)_sharedData;
        }
    }

}
