using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PageNavigationSample.Sample1.NavigationService;

namespace PageNavigationSample.Sample1
{
    public class MainController
    {
        private readonly NavigationService _navigationService;
        private readonly Panel _mainPanel;
        private PageKey _homePageKey;
        private object _sharedData;

        public MainController(Panel mainPanel, NavigationMode defaultMode)
        {
            _mainPanel = mainPanel;
            _navigationService = NavigationService.GetInstance(defaultMode);
            _sharedData = null;

            _navigationService.OnNavigate += UpdateUI;
        }

        public void SetHome(PageKey pageKey)
        {
            _homePageKey = pageKey;
        }

        public void RegisterPage(PageKey key, Type pageType)
        {
            _navigationService.RegisterPage(key, pageType);
        }

        public void NavigateTo(PageKey key)
        {
            _navigationService.Navigate(key, _sharedData);
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public void Cancel()
        {
            // キャンセル時の特定のロジックがあればここに追加
            GoHome(); // キャンセル時はホームに戻る例
        }

        public void GoHome()
        {
            _navigationService.Navigate(_homePageKey, _sharedData);
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
            var breadcrumbs = _navigationService.GetBreadcrumbs();
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
