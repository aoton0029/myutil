using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintSample
{
    public class NavigationService
    {
        private static NavigationService _instance;
        private readonly Stack<UcPage> _backStack = new Stack<UcPage>();
        private readonly Stack<UcPage> _forwardStack = new Stack<UcPage>();
        private Frame _frame;

        public static NavigationService Instance => _instance ??= new NavigationService();

        public void Initialize(Frame frame)
        {
            _frame = frame;
        }

        public void Navigate(UcPage page, object parameter = null)
        {
            if (_frame == null) throw new InvalidOperationException("Frame is not initialized.");

            if (_frame.Controls.Count > 0 && _frame.Controls[0] is UcPage currentPage)
            {
                _backStack.Push(currentPage);
                _forwardStack.Clear();
            }

            _frame.Navigate(page, parameter);
        }

        public void GoBack()
        {
            if (_backStack.Count > 0)
            {
                var previousPage = _backStack.Pop();
                _forwardStack.Push(_frame.Controls[0] as UcPage);
                _frame.Navigate(previousPage);
            }
        }

        public void GoForward()
        {
            if (_forwardStack.Count > 0)
            {
                var nextPage = _forwardStack.Pop();
                _backStack.Push(_frame.Controls[0] as UcPage);
                _frame.Navigate(nextPage);
            }
        }
    }
}
