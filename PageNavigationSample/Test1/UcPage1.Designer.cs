
namespace PageNavigationSample
{
    partial class UcPage1
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            button2 = new Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Font = new Font("Yu Gothic UI", 14.25F);
            button1.Location = new Point(77, 45);
            button1.Name = "button1";
            button1.Size = new Size(104, 49);
            button1.TabIndex = 0;
            button1.Text = "2";
            button1.UseVisualStyleBackColor = true;
            button1.Click += this.button1_Click;
            // 
            // button2
            // 
            button2.Font = new Font("Yu Gothic UI", 14.25F);
            button2.Location = new Point(187, 45);
            button2.Name = "button2";
            button2.Size = new Size(104, 49);
            button2.TabIndex = 1;
            button2.Text = "3";
            button2.UseVisualStyleBackColor = true;
            button2.Click += this.button2_Click;
            // 
            // UcPage1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "UcPage1";
            Size = new Size(673, 366);
            ResumeLayout(false);
        }


        #endregion

        private Button button1;
        private Button button2;
    }
}
