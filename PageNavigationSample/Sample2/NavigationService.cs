using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Sample2
{
    public class NavigationService
    {
        private static NavigationService _instance;
        private Panel _mainPanel;
        private UserControl _currentControl;
        private Stack<HistoryItem> _backHistory = new Stack<HistoryItem>();
        private Stack<HistoryItem> _forwardHistory = new Stack<HistoryItem>();

        private NavigationService() { }

        public static NavigationService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NavigationService();
                return _instance;
            }
        }

        public void Initialize(Panel mainPanel)
        {
            _mainPanel = mainPanel;
        }

        public void Navigate(UserControl newControl, string pageName, object parameter = null)
        {
            if (_currentControl != null)
            {
                // 現在のページを履歴に追加
                _backHistory.Push(new HistoryItem(pageName, parameter, _currentControl));
                _forwardHistory.Clear(); // 新しいナビゲーションが行われたとき、進む履歴をクリア
                _mainPanel.Controls.Remove(_currentControl);
            }

            _currentControl = newControl;
            _mainPanel.Controls.Add(_currentControl);
            _currentControl.Dock = DockStyle.Fill;
        }

        public void GoBack()
        {
            if (_backHistory.Count > 0)
            {
                // 現在のページを進む履歴に追加
                _forwardHistory.Push(new HistoryItem("", null, _currentControl));
                // 戻る履歴から最後のページを取得
                var historyItem = _backHistory.Pop();
                _mainPanel.Controls.Remove(_currentControl);
                _currentControl = historyItem.Page;
                _mainPanel.Controls.Add(_currentControl);
                _currentControl.Dock = DockStyle.Fill;
            }
        }

        public void GoForward()
        {
            if (_forwardHistory.Count > 0)
            {
                // 現在のページを戻る履歴に追加
                _backHistory.Push(new HistoryItem("", null, _currentControl));
                // 進む履歴から最後のページを取得
                var historyItem = _forwardHistory.Pop();
                _mainPanel.Controls.Remove(_currentControl);
                _currentControl = historyItem.Page;
                _mainPanel.Controls.Add(_currentControl);
                _currentControl.Dock = DockStyle.Fill;
            }
        }

        public void ClearHistory()
        {

        }
    }
}
