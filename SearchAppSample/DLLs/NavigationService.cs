using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SearchAppSample
{

    public struct NavigationResult
    {
        public bool ShouldClose { get; set; }               // ナビゲーションを終了して画面を閉じるか
        public Type? RedirectToPage { get; set; }           // 終了後、遷移すべきページ（nullで何もしない）
        public object[]? TempData { get; set; }             // 次ページへ渡すデータ（RedirectToPageがある場合）

        public static NavigationResult Close() => new() { ShouldClose = true };
        public static NavigationResult None() => new() { ShouldClose = false };
        public static NavigationResult Redirect<T>(params object[] data) => new()
        {
            RedirectToPage = typeof(T),
            TempData = data
        };
    }

    public class NavigationContext
    {
        public Type? PrevPage { get; set; }
        public Type? CurrentPage { get; set; }
        public Type? DefaultNextPage { get; set; }
        public Type? NextPage { get; set; }
        public object[] TempData { get; set; }
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

        public Func<NavigationContext, NavigationResult> OnCancel;
        public Func<NavigationContext, NavigationResult> OnComplete;

        public NavigationFlowService(Control host, ServiceProvider provider, Func<NavigationContext, NavigationResult> aOnCancel, Func<NavigationContext, NavigationResult> aOnComplete) : base(host, provider)
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
            var from = _currentPage?.GetType();
            var to = typeof(T);
            var prev = _history.Count > 0 ? _history.Peek() : null;

            Context.TempData = aTempData;
            Context.DefaultNextPage = to;
            Context.NextPage = to;

            var uc_from = _currentPage;
            var uc_to = (UserControl)_provider.Resolve(to);

            _history.Push(to);
            InternalNavigateTo(uc_from, uc_to, prev, from, to);
        }

        public void GoPrev(params object[] aTempData)
        {
            if (_history.Count <= 1) return;

            _history.Pop(); // 現在のページを捨てる
            var to = _history.Peek(); // 一つ前を取得

            var from = _currentPage?.GetType();
            var prev = _history.Count > 1 ? _history.ToArray()[1] : null;

            Context.TempData = aTempData;
            Context.DefaultNextPage = to;
            Context.NextPage = to;

            var uc_from = _currentPage;
            var uc_to = (UserControl)_provider.Resolve(to);

            InternalNavigateTo(uc_from, uc_to, prev, from, to);
        }


        public void Cancel(params object[] aTempData)
        {
            Context.TempData = aTempData;
            var result = OnCancel(Context);

            if (result.ShouldClose)
            {
                _container.Controls.Clear();
                _history.Clear();
            }

            if (result.RedirectToPage != null)
            {
                var uc_from = _currentPage;
                var uc_to = (UserControl)_provider.Resolve(result.RedirectToPage);
                InternalNavigateTo(uc_from, uc_to, _currentPage?.GetType(), _currentPage?.GetType(), result.RedirectToPage);
            }
        }

        public void Complete(params object[] aTempData)
        {
            Context.TempData = aTempData;
            var result = OnComplete(Context);

            if (result.ShouldClose)
            {
                _container.Controls.Clear();
                _history.Clear();
            }

            if (result.RedirectToPage != null)
            {
                var uc_from = _currentPage;
                var uc_to = (UserControl)_provider.Resolve(result.RedirectToPage);
                InternalNavigateTo(uc_from, uc_to, _currentPage?.GetType(), _currentPage?.GetType(), result.RedirectToPage);
            }
        }

    }


}
