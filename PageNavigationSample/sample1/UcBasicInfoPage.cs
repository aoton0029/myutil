using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PageNavigationSample.sample1
{
    public partial class UcBasicInfoPage : UcFlowPageBase, ISnapshot
    {
        private readonly RegistrationModel _model;

        public UcBasicInfoPage(NavigationFlowService navigationService) : base(navigationService)
        {
            InitializeComponent();
        }

        public override void OnPageShown(NavigationContext context)
        {
            base.OnPageShown(context);

            // モデルの取得または作成
            _model = context.TempData.GetValue<RegistrationModel>("RegistrationModel") ?? new RegistrationModel();

            // UIにデータをバインド
            txtName.Text = _model.Name;
            txtEmail.Text = _model.Email;
            dtpBirthdate.Value = _model.Birthdate;
        }

        public override void OnPageLeave(NavigationContext context)
        {
            base.OnPageLeave(context);

            // UIからデータを取得
            _model.Name = txtName.Text;
            _model.Email = txtEmail.Text;
            _model.Birthdate = dtpBirthdate.Value;

            // コンテキストにモデルを設定
            context.TempData.SetValue("RegistrationModel", _model);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            // 入力検証
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("名前を入力してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@"))
            {
                MessageBox.Show("有効なメールアドレスを入力してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 次のページに進む
            NavigationParameter param = new NavigationParameter();
            param.SetValue("RegistrationModel", _model);
            NavigationService.GoNext<UcAddressPage>(param);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            NavigationService.Cancel(new NavigationParameter());
        }

        public object CreateSnapshot()
        {
            // UIの状態をスナップショット
            return new
            {
                NameText = txtName.Text,
                EmailText = txtEmail.Text,
                BirthdateValue = dtpBirthdate.Value
            };
        }

        public void RestoreFromData(object data)
        {
            // スナップショットからUIを復元
            dynamic snapshot = data;
            txtName.Text = snapshot.NameText;
            txtEmail.Text = snapshot.EmailText;
            dtpBirthdate.Value = snapshot.BirthdateValue;
        }
    }
}
