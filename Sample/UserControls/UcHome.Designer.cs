namespace Sample.UserControls
{
    partial class UcHome
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
            button4 = new Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top;
            button1.Font = new Font("メイリオ", 27.75F);
            button1.Location = new Point(208, 104);
            button1.Name = "button1";
            button1.Size = new Size(375, 493);
            button1.TabIndex = 0;
            button1.Text = "製品一覧";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Top;
            button2.Font = new Font("メイリオ", 27.75F);
            button2.Location = new Point(609, 104);
            button2.Name = "button2";
            button2.Size = new Size(483, 241);
            button2.TabIndex = 1;
            button2.Text = "製品管理";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Top;
            button4.Font = new Font("メイリオ", 27.75F);
            button4.Location = new Point(609, 356);
            button4.Name = "button4";
            button4.Size = new Size(483, 241);
            button4.TabIndex = 3;
            button4.Text = "オプション管理";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // UcHome
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(button4);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "UcHome";
            Size = new Size(1300, 700);
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button4;
    }
}
