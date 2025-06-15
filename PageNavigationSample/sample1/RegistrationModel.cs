using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.sample1
{
    /// <summary>
    /// 登録プロセスで使用するデータモデル
    /// </summary>
    public class RegistrationModel : ISnapshot
    {
        // 基本情報
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Birthdate { get; set; }

        // 住所情報
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }

        // 追加情報
        public string PhoneNumber { get; set; }
        public string Occupation { get; set; }

        // 設定情報
        public bool ReceiveNewsletter { get; set; }
        public bool AgreeToTerms { get; set; }

        public RegistrationModel()
        {
            // デフォルト値で初期化
            Name = string.Empty;
            Email = string.Empty;
            Birthdate = DateTime.Now.AddYears(-20);
            Address = string.Empty;
            City = string.Empty;
            PostalCode = string.Empty;
            PhoneNumber = string.Empty;
            Occupation = string.Empty;
            ReceiveNewsletter = false;
            AgreeToTerms = false;
        }

        /// <summary>
        /// 現在のモデル状態のスナップショットを作成
        /// </summary>
        public object CreateSnapshot()
        {
            // ディープコピーを作成して返す
            return new RegistrationModel
            {
                Name = this.Name,
                Email = this.Email,
                Birthdate = this.Birthdate,
                Address = this.Address,
                City = this.City,
                PostalCode = this.PostalCode,
                PhoneNumber = this.PhoneNumber,
                Occupation = this.Occupation,
                ReceiveNewsletter = this.ReceiveNewsletter,
                AgreeToTerms = this.AgreeToTerms
            };
        }

        /// <summary>
        /// スナップショットからデータを復元
        /// </summary>
        public void RestoreFromData(object data)
        {
            if (data is RegistrationModel snapshot)
            {
                Name = snapshot.Name;
                Email = snapshot.Email;
                Birthdate = snapshot.Birthdate;
                Address = snapshot.Address;
                City = snapshot.City;
                PostalCode = snapshot.PostalCode;
                PhoneNumber = snapshot.PhoneNumber;
                Occupation = snapshot.Occupation;
                ReceiveNewsletter = snapshot.ReceiveNewsletter;
                AgreeToTerms = snapshot.AgreeToTerms;
            }
        }
    }
}
