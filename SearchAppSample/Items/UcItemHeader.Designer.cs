namespace SearchAppSample.Items
{
    partial class UcItemHeader
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
            lblTitle = new Label();
            btnBack = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Left;
            lblTitle.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(408, 33);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Title";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnBack
            // 
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnBack.FlatAppearance.BorderColor = Color.FromArgb(54, 100, 139);
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnBack.ForeColor = Color.White;
            btnBack.Location = new Point(1197, 0);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(103, 33);
            btnBack.TabIndex = 1;
            btnBack.Text = "戻る";
            btnBack.UseVisualStyleBackColor = true;
            // 
            // UcItemHeader
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(43, 79, 107);
            Controls.Add(btnBack);
            Controls.Add(lblTitle);
            Name = "UcItemHeader";
            Size = new Size(1300, 33);
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitle;
        private Button btnBack;
    }
}
