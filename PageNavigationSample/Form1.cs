using PageNavigationSample.sample1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PageNavigationSample
{
    public partial class Form1 : Form
    {
        private readonly NavigationFlowService _navigationService;
        private readonly RegistrationModel _registrationModel;
        private readonly BreadCrumb _breadCrumb;
        private readonly SnapshotManager _snapshotManager;
        private readonly Panel _contentPanel;
        private readonly UcBreadCrumbControl _breadCrumbControl;

        public Form1()
        {
            InitializeComponent();
            // コンポーネントの初期化
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            _breadCrumbControl = new UcBreadCrumbControl
            {
                Dock = DockStyle.Top,
                Height = 50
            };

            // フォームにコントロールを追加
            Controls.Add(_contentPanel);
            Controls.Add(_breadCrumbControl);

            // モデルとナビゲーション管理の初期化
            _registrationModel = new RegistrationModel();
            _snapshotManager = new SnapshotManager(_registrationModel);
            _breadCrumb = new BreadCrumb();

            // パンくずリストの設定
            _breadCrumb.AddItem("基本情報", typeof(UcBasicInfoPage));
            _breadCrumb.AddItem("住所情報", typeof(UcAddressPage));
            _breadCrumb.AddItem("追加情報", typeof(UcAdditionalInfoPage));
            _breadCrumb.AddItem("確認", typeof(UcConfirmationPage));
            _breadCrumbControl.SetBreadCrumb(_breadCrumb);

            // コンテキストの作成
            var context = new NavigationContext
            {
                BreadCrumb = _breadCrumb,
                TempData = new NavigationParameter()
            };

            // NavigationFlowServiceの初期化
            _navigationService = new NavigationFlowService(
                _contentPanel,
                context,
                _breadCrumb,
                _snapshotManager,
                OnCancelRegistration,
                OnCompleteRegistration,
                OnTerminateRegistration
            );

            // イベントハンドラを設定
            _navigationService.BeforeNavigating += NavigationService_BeforeNavigating;
            _navigationService.AfterNavigated += NavigationService_AfterNavigated;

            // 最初のページに移動
            _navigationService.GoNext<UcBasicInfoPage>();
        }

        private void NavigationService_BeforeNavigating(object sender, NavigationEventArgs e)
        {
            // 遷移前の処理（例：ログの記録）
            Console.WriteLine($"遷移開始: {e.FromPage?.Name} から {e.ToPage?.Name} へ");
        }

        private void NavigationService_AfterNavigated(object sender, NavigationEventArgs e)
        {
            // 遷移後の処理（例：パンくずリストの更新）
            _breadCrumbControl.RefreshBreadCrumb();
            Console.WriteLine($"遷移完了: {e.ToPage?.Name} に移動しました");
        }

        private NavigationResult OnCancelRegistration(NavigationContext context)
        {
            // キャンセル処理
            var result = MessageBox.Show(
                "登録をキャンセルしますか？",
                "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                return NavigationResult.Close();
            }
            return NavigationResult.None();
        }

        private NavigationResult OnCompleteRegistration(NavigationContext context)
        {
            // 登録完了処理
            MessageBox.Show(
                "登録が完了しました！",
                "完了",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return NavigationResult.Close();
        }

        private NavigationResult OnTerminateRegistration(NavigationContext context)
        {
            // 強制終了処理
            return NavigationResult.Close();
        }
    }
}
