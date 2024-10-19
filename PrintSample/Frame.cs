using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintSample
{
    public class Frame : Panel
    {
        private UcPage _currentPage;

        public void Navigate(UcPage page, object parameter = null)
        {
            if (_currentPage != null)
            {
                _currentPage.OnNavigatedFrom();
                Controls.Remove(_currentPage);
            }

            _currentPage = page;
            if (_currentPage != null)
            {
                _currentPage.OnNavigatedTo(parameter);
                _currentPage.Dock = DockStyle.Fill;
                Controls.Add(_currentPage);
            }
        }

        public void GoBack()
        {
            // NavigationServiceと連携して履歴から戻る処理を実装
            NavigationService.Instance.GoBack();
        }

        public void GoForward()
        {
            // NavigationServiceと連携して進む処理を実装
            NavigationService.Instance.GoForward();
        }
    }
}
