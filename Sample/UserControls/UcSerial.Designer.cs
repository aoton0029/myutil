namespace Sample
{
    partial class UcSerial
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
            txtSend = new TextBox();
            cmbPort = new ComboBox();
            btnSend = new Button();
            txtRecv = new TextBox();
            SuspendLayout();
            // 
            // txtSend
            // 
            txtSend.Font = new Font("メイリオ", 12F);
            txtSend.Location = new Point(7, 57);
            txtSend.Name = "txtSend";
            txtSend.Size = new Size(258, 31);
            txtSend.TabIndex = 0;
            // 
            // cmbPort
            // 
            cmbPort.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPort.FlatStyle = FlatStyle.Flat;
            cmbPort.Font = new Font("メイリオ", 12F);
            cmbPort.FormattingEnabled = true;
            cmbPort.Location = new Point(7, 9);
            cmbPort.Name = "cmbPort";
            cmbPort.Size = new Size(258, 32);
            cmbPort.TabIndex = 1;
            // 
            // btnSend
            // 
            btnSend.Font = new Font("メイリオ", 12F);
            btnSend.Location = new Point(269, 57);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(75, 31);
            btnSend.TabIndex = 2;
            btnSend.Text = "送信";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // txtRecv
            // 
            txtRecv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtRecv.Font = new Font("メイリオ", 12F);
            txtRecv.Location = new Point(7, 94);
            txtRecv.Multiline = true;
            txtRecv.Name = "txtRecv";
            txtRecv.ScrollBars = ScrollBars.Vertical;
            txtRecv.Size = new Size(337, 302);
            txtRecv.TabIndex = 3;
            // 
            // UcSerial
            // 
            AutoScaleMode = AutoScaleMode.None;
            Controls.Add(txtRecv);
            Controls.Add(btnSend);
            Controls.Add(cmbPort);
            Controls.Add(txtSend);
            Name = "UcSerial";
            Size = new Size(350, 399);
            Load += UcSerial_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtSend;
        private ComboBox cmbPort;
        private Button btnSend;
        private TextBox txtRecv;
    }
}
