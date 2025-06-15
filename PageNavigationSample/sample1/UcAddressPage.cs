using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PageNavigationSample.sample1
{
    public partial class UcAddressPage : UcFlowPageBase, ISnapshot
    {
        private readonly RegistrationModel _model;

        public UcAddressPage(NavigationFlowService navigationService) : base(navigationService)
        {
            InitializeComponent();
        }

        public override void OnPageShown(NavigationContext context)
        {
            base.OnPageShown(context);

            // モデルの取得
            _model = context.TempData.GetValue<RegistrationModel>("RegistrationModel");

            // UIにデータをバインド
            txtAddress.Text = _model.Address;
            txtCity.Text = _model.City;
            txtPostalCode.Text = _model.PostalCode;
        }

        public override void OnPageLeave(NavigationContext context)
        {
            base.OnPageLeave(context);

            // UIからデータを取得
            _model.Address = txtAddress.Text;
            _model.City = txtCity.Text;
            _model.PostalCode = txtPostalCode.Text;

            // コンテキストにモデルを設定
            context.TempData.SetValue("RegistrationModel", _model);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            // 入力検証
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("住所を入力してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCity.Text))
            {
                MessageBox.Show("市区町村を入力してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPostalCode.Text))
            {
                MessageBox.Show("郵便番号を入力してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 次のページに進む
            NavigationParameter param = new NavigationParameter();
            param.SetValue("RegistrationModel", _model);
            NavigationService.GoNext<UcAdditionalInfoPage>(param);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            NavigationService.GoPrevious();
        }

        public object CreateSnapshot()
        {
            // UIの状態をスナップショット
            return new
            {
                AddressText = txtAddress.Text,
                CityText = txtCity.Text,
                PostalCodeText = txtPostalCode.Text
            };
        }

        public void RestoreFromData(object data)
        {
            // スナップショットからUIを復元
            dynamic snapshot = data;
            txtAddress.Text = snapshot.AddressText;
            txtCity.Text = snapshot.CityText;
            txtPostalCode.Text = snapshot.PostalCodeText;
        }
    }
}
