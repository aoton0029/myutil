using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PageNavigationSample.Test2
{
    public class NavigationContext
    {
        public Type? PrevPage { get; set; }
        public Type? CurrentPage { get; set; }
        public Type? DefaultNextPage { get; set; }
        public Type? NextPage { get; set; }
        public object[] TempData { get; set; }
        public Dictionary<string, object> SharedData { get; set; } = new();

        public T? Get<T>(string key)
        {
            return SharedData.TryGetValue(key, out var value) ? (T?)value : default;
        }

        public void Set<T>(string key, T value)
        {
            SharedData[key] = value!;
        }
    }

    interface IPage 
    {
        void OnPageShown(NavigationContext context);
        void OnPageLeave(NavigationContext context);
    }

    interface IDecideNavigation
    {
        Type DecideNextPage(NavigationContext context);
    }

    public class NavigationService
    {
        protected readonly Control _container;
        protected UserControl _currentPage;
        protected readonly ServiceProvider _provider;

        public NavigationContext Context { get; private set; } = new();

        public delegate void NavigationEventHandler(NavigationContext context);
        public NavigationEventHandler PreNavigationEvent;
        public NavigationEventHandler PostNavigationEvent;

        public NavigationService(Control host, ServiceProvider provider)
        {
            _container = host;
            _provider = provider;
        }

        public void NavigateTo<T>(params object[] aTempData)
        {
            Type? from = _currentPage == null ? null : _currentPage.GetType();
            Type default_to = typeof(T);

            Context.TempData = aTempData;
            Context.PrevPage = null;
            Context.CurrentPage = from;
            Context.DefaultNextPage = default_to;
            Context.NextPage = default_to;

            Type to = _currentPage is IDecideNavigation decider
                        ? decider.DecideNextPage(Context)
                        : default_to;

            UserControl uc_from = _currentPage;
            UserControl uc_to = (UserControl)_provider.Resolve(to);
            InternalNavigateTo(uc_from, uc_to, null, from, to);
        }

        protected void InternalNavigateTo(UserControl uc_from, UserControl uc_to, Type? prev, Type? from, Type? to)
        {
            Context.PrevPage = prev;
            Context.CurrentPage = from;
            Context.NextPage = to;

            PreNavigationEvent?.Invoke(Context);

            if (uc_from is IPage p_from) p_from.OnPageLeave(Context);

            Context.PrevPage = from;
            Context.CurrentPage = to;
            Context.DefaultNextPage = null;
            Context.NextPage = null;

            _container.Controls.Clear();
            uc_to.Dock = DockStyle.Fill;
            _container.Controls.Add(uc_to);


            if (uc_to is IPage p_to) p_to.OnPageShown(Context);

            PostNavigationEvent?.Invoke(Context);
        }
    }

    public class NavigationFlowService : NavigationService
    {
        private readonly Stack<Type> _history = new();

        public Func<NavigationContext, bool> OnCancel;
        public Func<NavigationContext, bool> OnComplete;

        public NavigationFlowService(Control host, ServiceProvider provider, Func<NavigationContext, bool> aOnCancel, Func<NavigationContext, bool> aOnComplete) : base(host, provider)
        {
            OnCancel = aOnCancel;
            OnComplete = aOnComplete;
        }

        public void Start<T>() where T : UserControl
        {
            _history.Clear();
            GoNext<T>();
        }

        public void GoNext<T>(params object[] aTempData) where T : UserControl
        {
            Type? from = _currentPage?.GetType();
            Type default_to = typeof(T);

            Context.TempData = aTempData;
            Context.PrevPage = _history.Count > 0 ? _history.Peek() : null;
            Context.CurrentPage = from;
            Context.DefaultNextPage = default_to;
            Context.NextPage = default_to;

            Type to = _currentPage is IDecideNavigation decider
                        ? decider.DecideNextPage(Context)
                        : default_to;

            UserControl uc_from = _currentPage;
            UserControl uc_to = (UserControl)_provider.Resolve(to);

            _history.Push(typeof(T));
            InternalNavigateTo(uc_from, uc_to, prev, from, to);
        }

        public void GoPrev(params object[] aTempData)
        {
            if (_history.Count <= 1) { return; }
            Type from = _history.Pop(); // 現在のページ
            Type to = _history.Peek();  // 一つ前のページ

            Context.TempData = aTempData;
            Context.PrevPage = from;
            Context.CurrentPage = to;
            Context.DefaultNextPage = null;
            Context.NextPage = to;

            UserControl uc_from = _currentPage;
            UserControl uc_to = (UserControl)_provider.Resolve(to);

            InternalNavigateTo(uc_from, uc_to, prev, from, to);
        }


        public void Cancel(params object[] aTempData)
        {
            Context.TempData = aTempData;
            bool? b = OnCancel(Context);
            if(b.HasValue && b.Value)
            {
                _container.Controls.Clear();
                _history.Clear();
            }
        }

        public void Complete(params object[] aTempData)
        {
            Context.TempData = aTempData;
            bool? b = OnComplete(Context);
            if (b.HasValue && b.Value)
            {
                _container.Controls.Clear();
                _history.Clear();
            }
        }
    }


}
