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
    public partial class UcComfirmationPage : UcFlowPageBase, ISnapshot
    {
        private RegistrationModel _model;

        // コンストラクタ、イベントハンドラ、InitializeComponentは省略

        public override void OnPageShown(NavigationContext context)
        {
            _model = context.TempData.GetValue<RegistrationModel>("RegistrationModel");

            // 登録情報を表示用のリッチテキストボックスに表示
            rtbSummary.Clear();
            rtbSummary.AppendText($"【基本情報】\n");
            rtbSummary.AppendText($"お名前: {_model.Name}\n");
            rtbSummary.AppendText($"メールアドレス: {_model.Email}\n");
            rtbSummary.AppendText($"生年月日: {_model.Birthdate:yyyy/MM/dd}\n\n");

            rtbSummary.AppendText($"【住所情報】\n");
            rtbSummary.AppendText($"住所: {_model.Address}\n");
            rtbSummary.AppendText($"市区町村: {_model.City}\n");
            rtbSummary.AppendText($"郵便番号: {_model.PostalCode}\n\n");

            rtbSummary.AppendText($"【追加情報】\n");
            rtbSummary.AppendText($"電話番号: {_model.PhoneNumber}\n");
            rtbSummary.AppendText($"職業: {_model.Occupation}\n");
            rtbSummary.AppendText($"メールマガジン: {(_model.ReceiveNewsletter ? "希望する" : "希望しない")}\n");

            // 利用規約同意チェックボックス
            chkAgree.Checked = _model.AgreeToTerms;
        }

        public override void OnPageLeave(NavigationContext context)
        {
            _model.AgreeToTerms = chkAgree.Checked;
            context.TempData.SetValue("RegistrationModel", _model);
        }

        public object CreateSnapshot()
        {
            throw new NotImplementedException();
        }

        public void RestoreFromData(object data)
        {
            throw new NotImplementedException();
        }
    }
}
