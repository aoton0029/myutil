using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Test2
{
    public class NavigationFlowService
    {
        private readonly Control _host;
        private readonly ServiceProvider _provider;
        private readonly Stack<UserControl> _history = new();

        public NavigationContext CurrentContext { get; private set; } = new();

        public event Action? OnFlowCancelled;
        public event Action? OnFlowCompleted;

        public NavigationFlowService(Control host, ServiceProvider provider)
        {
            _host = host;
            _provider = provider;
        }

        public void Start<TStartPage>() where TStartPage : UserControl
        {
            _history.Clear();
            NavigateToPage(typeof(TStartPage), null);
        }

        public void GoNext<TDefaultNextPage>(object? parameter = null) where TDefaultNextPage : UserControl
        {
            if (_host.Controls.Count == 0) return;

            var currentPage = _host.Controls[0];
            var currentType = currentPage.GetType();
            var defaultNext = typeof(TDefaultNextPage);

            var context = new NavigationContext
            {
                CurrentPage = currentType,
                DefaultNextPage = defaultNext,
                Parameter = parameter,
                SharedData = CurrentContext.SharedData
            };

            var nextPage = currentPage is INextPageDecider decider
                ? decider.DecideNextPage(context)
                : defaultNext;

            if (nextPage == null)
            {
                Complete();
                return;
            }

            _history.Push((UserControl)currentPage);
            NavigateToPage(nextPage, context);
        }

        public void GoBack()
        {
            if (_history.Count > 0)
            {
                var prev = _history.Pop();
                _host.Controls.Clear();
                _host.Controls.Add(prev);
                prev.Dock = DockStyle.Fill;

                if (prev is IShown aware)
                    aware.OnShown(CurrentContext);
            }
        }

        public void Cancel()
        {
            _host.Controls.Clear();
            _history.Clear();
            OnFlowCancelled?.Invoke();
        }

        public void Complete()
        {
            _host.Controls.Clear();
            _history.Clear();
            OnFlowCompleted?.Invoke();
        }

        private void NavigateToPage(Type type, NavigationContext? context)
        {
            var page = (UserControl)_provider.Resolve(type);
            _host.Controls.Clear();
            _host.Controls.Add(page);
            page.Dock = DockStyle.Fill;

            CurrentContext = context ?? new NavigationContext { CurrentPage = type };

            if (page is IShown aware)
                aware.OnShown(CurrentContext);
        }
    }

}
