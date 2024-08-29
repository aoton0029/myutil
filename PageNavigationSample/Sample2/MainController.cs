using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Sample2
{
    public class MainController : BaseController
    {
        public InputData CurrentData { get; private set; }

        public MainController()
        {
            CurrentData = new InputData();
        }

        public void NavigateToStep1()
        {
            UserControl1 step1 = new UserControl1(this);
            _navigationService.Navigate(step1, "Step1");
        }

        public override void NavigateToHome()
        {
            // ホームページへの遷移を実装
            UcHome homePage = new UcHome(this);
            _navigationService.Navigate(homePage, "HomePage");
        }

        public override void Cancel()
        {
            // キャンセル時の処理を実装
            // 例: ナビゲーション履歴をクリアし、ホームに戻る
            _navigationService.ClearHistory();
            NavigateToHome();
        }
    }

    public class InputData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
