using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample
{
    public class RegisterFormNavigationService : NavigationService
    {
        private Stack<Type> _history = new(); // 画面の履歴
        private Action<object> _onComplete; // 完了時のコールバック
        private object _formData; // 入力データ

        public RegisterFormNavigationService(Panel container, object sharedData, Action<object> onComplete)
            : base(container, sharedData)
        {
            _onComplete = onComplete;
            _formData = new Dictionary<string, object>(); // 入力データ用
        }

        public void Start<T>() where T : UserControl
        {
            _history.Clear();
            Navigate(typeof(T));
        }

        public void Next(Type nextPageType)
        {
            if (!typeof(UserControl).IsAssignableFrom(nextPageType))
                throw new ArgumentException("Next page must be a UserControl.");

            _history.Push(_currentPage?.GetType());
            Navigate(nextPageType);
        }

        public void Previous()
        {
            if (_history.Count > 0)
            {
                Type previousPage = _history.Pop();
                Navigate(previousPage);
            }
        }

        public void Cancel()
        {
            if (_history.Count > 0)
            {
                Type firstPage = _history.ToArray()[^1]; // 履歴の最初のページ
                _history.Clear();
                Navigate(firstPage);
            }
        }

        public void Complete()
        {
            _onComplete?.Invoke(_formData);
        }

        public void SaveData(string key, object value)
        {
            if (_formData is Dictionary<string, object> data)
            {
                data[key] = value;
            }
        }

        public object GetData(string key)
        {
            if (_formData is Dictionary<string, object> data && data.ContainsKey(key))
            {
                return data[key];
            }
            return null;
        }
    }
}
