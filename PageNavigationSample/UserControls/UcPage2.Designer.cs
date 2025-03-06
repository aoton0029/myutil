namespace PageNavigationSample
{
    partial class UcPage2
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
            button2 = new Button();
            button1 = new Button();
            SuspendLayout();
            // 
            // button2
            // 
            button2.Font = new Font("Yu Gothic UI", 14.25F);
            button2.Location = new Point(148, 21);
            button2.Name = "button2";
            button2.Size = new Size(104, 49);
            button2.TabIndex = 3;
            button2.Text = "3";
            button2.UseVisualStyleBackColor = true;
            button2.Click += this.button2_Click;
            // 
            // button1
            // 
            button1.Font = new Font("Yu Gothic UI", 14.25F);
            button1.Location = new Point(38, 21);
            button1.Name = "button1";
            button1.Size = new Size(104, 49);
            button1.TabIndex = 2;
            button1.Text = "1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += this.button1_Click;
            // 
            // UcPage2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(192, 255, 192);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "UcPage2";
            Size = new Size(626, 370);
            ResumeLayout(false);
        }

        #endregion

        private Button button2;
        private Button button1;
    }
}
