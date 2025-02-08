using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample
{
    public class NavigationService : IDisposable
    {
        private readonly Panel _container;
        private readonly Dictionary<Type, Func<UserControl>> _pages = new();
        private object _sharedData;
        private object _tempData;
        private UserControl _currentPage;
        private bool _disposed = false;

        public NavigationService(Panel container, object sharedData)
        {
            _container = container;
            _sharedData = sharedData;
        }

        public void Register<T>(Func<T> factory) where T : UserControl
        {
            _pages[typeof(T)] = () => factory();
        }

        public void Navigate<T>(object tempData = null) where T : UserControl
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NavigationService));

            if (_pages.TryGetValue(typeof(T), out var factory))
            {
                // 現在のページの参照を解除
                if (_currentPage is IDisposable disposablePage)
                {
                    disposablePage.Dispose();
                }
                _container.Controls.Clear();

                _tempData = tempData;
                _currentPage = factory();
                if (_currentPage is INavigablePage page)
                {
                    page.OnNavigated(_sharedData, _tempData);
                }

                _container.Controls.Add(_currentPage);
                _currentPage.Dock = DockStyle.Fill;
            }
            else
            {
                throw new InvalidOperationException($"Page {typeof(T).Name} is not registered.");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // 現在のページを解放
            if (_currentPage is IDisposable disposablePage)
            {
                disposablePage.Dispose();
            }
            _currentPage = null;

            // コンテナのコントロールをクリア
            _container.Controls.Clear();

            // 登録済みページのクリア
            _pages.Clear();
        }
    }
}
