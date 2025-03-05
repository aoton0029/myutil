namespace FormSample
{
    partial class UcTitleBar
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
            menuStrip1 = new MenuStrip();
            btnClose = new Button();
            btnMaximize = new Button();
            btnMinimize = new Button();
            picAppIcon = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)picAppIcon).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            menuStrip1.AutoSize = false;
            menuStrip1.Dock = DockStyle.None;
            menuStrip1.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            menuStrip1.Location = new Point(32, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(859, 32);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = SystemColors.ActiveCaptionText;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(955, 0);
            btnClose.Margin = new Padding(0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(32, 32);
            btnClose.TabIndex = 1;
            btnClose.Text = "×";
            btnClose.UseVisualStyleBackColor = false;
            // 
            // btnMaximize
            // 
            btnMaximize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaximize.BackColor = SystemColors.ActiveCaptionText;
            btnMaximize.FlatStyle = FlatStyle.Flat;
            btnMaximize.ForeColor = Color.White;
            btnMaximize.Location = new Point(923, 0);
            btnMaximize.Margin = new Padding(0);
            btnMaximize.Name = "btnMaximize";
            btnMaximize.Size = new Size(32, 32);
            btnMaximize.TabIndex = 2;
            btnMaximize.Text = "□";
            btnMaximize.UseVisualStyleBackColor = false;
            // 
            // btnMinimize
            // 
            btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimize.BackColor = SystemColors.ActiveCaptionText;
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.ForeColor = Color.White;
            btnMinimize.Location = new Point(891, 0);
            btnMinimize.Margin = new Padding(0);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(32, 32);
            btnMinimize.TabIndex = 3;
            btnMinimize.Text = "＿";
            btnMinimize.UseVisualStyleBackColor = false;
            // 
            // picAppIcon
            // 
            picAppIcon.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            picAppIcon.Location = new Point(0, 0);
            picAppIcon.Margin = new Padding(0);
            picAppIcon.Name = "picAppIcon";
            picAppIcon.Size = new Size(32, 32);
            picAppIcon.SizeMode = PictureBoxSizeMode.Zoom;
            picAppIcon.TabIndex = 4;
            picAppIcon.TabStop = false;
            // 
            // UcTitleBar
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(picAppIcon);
            Controls.Add(btnMinimize);
            Controls.Add(btnMaximize);
            Controls.Add(btnClose);
            Controls.Add(menuStrip1);
            Name = "UcTitleBar";
            Size = new Size(987, 32);
            ((System.ComponentModel.ISupportInitialize)picAppIcon).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private MenuStrip menuStrip1;
        private Button btnClose;
        private Button btnMaximize;
        private Button btnMinimize;
        private PictureBox picAppIcon;
    }
}
