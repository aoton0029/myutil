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
        private object _sharedData;  // SharedDataをobjectとして保持

        public MainController(Panel mainPanel, NavigationMode defaultMode)
        {
            _mainPanel = mainPanel;
            _navigationService = NavigationService.GetInstance(defaultMode);
            _sharedData = null;

            _navigationService.OnNavigate += UpdateUI;
        }

        public void RegisterPage(PageKey key, Type pageType)
        {
            _navigationService.RegisterPage(key, pageType);
        }

        public void NavigateTo(PageKey key, NavigationMode? mode = null)
        {
            _navigationService.Navigate(key, _sharedData, mode);
        }

        public void GoBack()
        {
            _navigationService.GoBack();
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
        }

        // object型のSharedDataを更新するメソッド
        public void UpdateSharedData(object newValue)
        {
            _sharedData = newValue;
        }

        // object型のSharedDataを取得するメソッド
        public T GetSharedData<T>()
        {
            return (T)_sharedData;
        }
    }
}
