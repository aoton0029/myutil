using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample
{
    public interface IPage
    {
        void OnPageShown(NavigationContext context);
        void OnPageLeave(NavigationContext context);
    }

    /// <summary>
    /// ナビゲーションのコンテキストを表すクラス
    /// </summary>
    public class NavigationContext
    {
        public Type? PrevPage { get; set; }
        public Type? CurrentPage { get; set; }
        public Type? NextPage { get; set; }
        public NavigationParameter TempData { get; set; }
        public BreadCrumb BreadCrumb { get; set; }
    }

    /// <summary>
    /// 画面間で一次的なデータを渡すためのパラメータクラス
    /// </summary>
    public class NavigationParameter : Dictionary<string, object>
    {
        public NavigationParameter() { }
        public NavigationParameter(string key, object value)
        {
            this[key] = value;
        }
        public T? GetValue<T>(string key)
        {
            if (TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default;
        }
        public void SetValue<T>(string key, T value)
        {
            this[key] = value;
        }
    }

    /// <summary>
    /// ナビゲーションの結果を表すクラス
    /// </summary>
    public class NavigationResult
    {
        public bool ShouldClose { get; set; }               // ナビゲーションを終了して画面を閉じるか
        public Type? RedirectToPage { get; set; }           // 終了後、遷移すべきページ（nullで何もしない）
        public NavigationParameter TempData { get; set; }             // 次ページへ渡すデータ（RedirectToPageがある場合）

        public static NavigationResult Close() => new() { ShouldClose = true };
        public static NavigationResult None() => new() { ShouldClose = false };
        public static NavigationResult Redirect<T>(NavigationParameter data) => new()
        {
            RedirectToPage = typeof(T),
            TempData = data
        };
    }


    /// <summary>
    /// ナビゲーションイベント引数クラス
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        /// <summary>
        /// ナビゲーションコンテキスト
        /// </summary>
        public NavigationContext Context { get; set; }

        /// <summary>
        /// 遷移元ページの型情報
        /// </summary>
        public Type FromPage { get; set; }

        /// <summary>
        /// 遷移先ページの型情報
        /// </summary>
        public Type ToPage { get; set; }

        /// <summary>
        /// 渡されるパラメータ
        /// </summary>
        public NavigationParameter Parameter { get; set; }

        /// <summary>
        /// 遷移をキャンセルするかどうか（遷移前イベントでのみ有効）
        /// </summary>
        public bool Cancel { get; set; }
    }

    public class NavigationFlowService
    {
        private readonly Control _container;
        private readonly BreadCrumb _breadCrumb;
        private readonly NavigationContext _context;
        private readonly SnapshotManager _snapshotManager;

        private UserControl _currentPage = null;
        private Stack<Type> _navigationHistory = new Stack<Type>();
        private Dictionary<Type, UserControl> _pageCache = new Dictionary<Type, UserControl>();

        private Func<NavigationContext, NavigationResult> OnCancel;
        private Func<NavigationContext, NavigationResult> OnTerminate;
        private Func<NavigationContext, NavigationResult> OnComplete;

        public event EventHandler<NavigationEventArgs> BeforeNavigating;

        public event EventHandler<NavigationEventArgs> AfterNavigated;

        public NavigationFlowService(
            Control container,
            NavigationContext context = null,
            BreadCrumb breadCrumb = null,
            SnapshotManager snapshotManager = null,
            Func<NavigationContext, NavigationResult> onCancel = null,
            Func<NavigationContext, NavigationResult> onComplete = null,
            Func<NavigationContext, NavigationResult> onTerminate = null)
        {
            _container = container;
            _breadCrumb = breadCrumb;
            _context = context;
            _snapshotManager = snapshotManager;
            OnCancel = onCancel ?? (ctx => NavigationResult.None());
            OnTerminate = onTerminate ?? (ctx => NavigationResult.None());
            OnComplete = onComplete ?? (ctx => NavigationResult.None());
        }

        /// <summary>
        /// 指定したページに遷移する内部メソッド
        /// </summary>
        /// <param name="pageType">遷移先のページタイプ</param>
        /// <param name="parameter">渡すパラメータ</param>
        /// <param name="takeSnapshot">スナップショットを取るかどうか</param>
        protected void InternalNavigateTo(Type pageType, NavigationParameter parameter, bool takeSnapshot = true)
        {
            // 遷移前イベントを発生させる
            var beforeArgs = new NavigationEventArgs
            {
                Context = _context,
                FromPage = _context.CurrentPage,
                ToPage = pageType,
                Parameter = parameter
            };
            OnBeforeNavigating(beforeArgs);

            // イベントがキャンセルされた場合は遷移しない
            if (beforeArgs.Cancel)
            {
                return;
            }

            // 現在のページがあれば、離脱処理を実行
            if (_currentPage != null)
            {
                // スナップショットを保存（必要に応じて）
                if (takeSnapshot && _snapshotManager != null && _currentPage is ISnapshot snapshotPage)
                {
                    _snapshotManager.SaveSnapshot();
                }

                // ページ離脱処理を実行
                if (_currentPage is IPage page)
                {
                    page.OnPageLeave(_context);
                }

                // 現在のコントロールをコンテナから削除
                _container.Controls.Remove(_currentPage);
            }

            // ナビゲーション履歴とコンテキストを更新
            _navigationHistory.Push(pageType);
            _context.PrevPage = _context.CurrentPage;
            _context.CurrentPage = pageType;
            _context.TempData = parameter ?? new NavigationParameter();

            // パンくずリストを更新
            if (_breadCrumb != null)
            {
                _breadCrumb.SetCurrentPosition(pageType);
            }

            // 新しいページを作成または取得してコンテナに追加
            if (!_pageCache.TryGetValue(pageType, out UserControl newPage))
            {
                // ページのインスタンスを作成
                newPage = (UserControl)Activator.CreateInstance(pageType);
                _pageCache[pageType] = newPage;
            }

            _currentPage = newPage;
            _currentPage.Dock = DockStyle.Fill;
            _container.Controls.Add(_currentPage);

            // ページ表示処理を実行
            if (_currentPage is IPage pageInterface)
            {
                pageInterface.OnPageShown(_context);
            }

            // 遷移後イベントを発生させる
            var afterArgs = new NavigationEventArgs
            {
                Context = _context,
                FromPage = _context.PrevPage,
                ToPage = _context.CurrentPage,
                Parameter = parameter
            };
            OnAfterNavigated(afterArgs);
        }

        /// <summary>
        /// 遷移前イベント発生メソッド
        /// </summary>
        protected virtual void OnBeforeNavigating(NavigationEventArgs e)
        {
            BeforeNavigating?.Invoke(this, e);
        }

        /// <summary>
        /// 遷移後イベント発生メソッド
        /// </summary>
        protected virtual void OnAfterNavigated(NavigationEventArgs e)
        {
            AfterNavigated?.Invoke(this, e);
        }

        /// <summary>
        /// 次のページに進む
        /// </summary>
        /// <param name="parameter">パラメータ</param>
        public void GoNext<T>(NavigationParameter parameter = null) where T : UserControl
        {
            Type nextPageType = typeof(T);
            _context.NextPage = nextPageType;

            // スナップショットを取る
            InternalNavigateTo(nextPageType, parameter);
        }

        /// <summary>
        /// 前のページに戻る
        /// </summary>
        /// <param name="parameter">パラメータ</param>
        public void GoPrevious(NavigationParameter parameter = null)
        {
            if (_navigationHistory.Count <= 1)
            {
                // 履歴が不足している場合は何もしない
                return;
            }

            // 現在のページを履歴から取り出す（現在表示中のため）
            _navigationHistory.Pop();

            // 前のページを取得
            Type prevPageType = _navigationHistory.Peek();
            _context.NextPage = null;

            // 前のページへ移動（スナップショットは取らない）
            InternalNavigateTo(prevPageType, parameter, false);

            // パンくずリストを更新（現在のページ以降を削除）
            if (_breadCrumb != null)
            {
                _breadCrumb.TruncateAfter(prevPageType);
            }

            // スナップショットから復元（ある場合）
            if (_snapshotManager != null && _currentPage is ISnapshot snapshotPage)
            {
                try
                {
                    _snapshotManager.RestoreFromSnapshot();
                }
                catch (InvalidOperationException)
                {
                    // スナップショットがない場合は何もしない
                }
            }
        }

        public void Cancel(NavigationParameter parameter)
        {
            var result = OnCancel(_context);
            handleNavigationResult(result);
        }

        public void Terminate(NavigationParameter parameter)
        {
            var result = OnTerminate(_context);
            handleNavigationResult(result);
        }

        public void Complete(NavigationParameter parameter)
        {
            var result = OnComplete(_context);
            handleNavigationResult(result);
        }

        private void handleNavigationResult(NavigationResult result)
        {
            if (result.ShouldClose)
            {
                // 画面を閉じる処理
                _container.Dispose();
                return;
            }
            if (result.RedirectToPage != null)
            {
                // 次のページへ遷移
                InternalNavigateTo(result.RedirectToPage, result.TempData);
            }
        }
    }
}
