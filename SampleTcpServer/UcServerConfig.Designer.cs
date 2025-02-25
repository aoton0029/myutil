namespace SampleTcpServer
{
    partial class UcServerConfig
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
            chkUse = new CheckBox();
            nud1 = new NumericUpDown();
            cmbDevice = new ComboBox();
            nud2 = new NumericUpDown();
            nud3 = new NumericUpDown();
            nud4 = new NumericUpDown();
            nudPort = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)nud1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nud2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nud3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nud4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPort).BeginInit();
            SuspendLayout();
            // 
            // chkUse
            // 
            chkUse.AutoSize = true;
            chkUse.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            chkUse.Location = new Point(3, 9);
            chkUse.Name = "chkUse";
            chkUse.Size = new Size(15, 14);
            chkUse.TabIndex = 0;
            chkUse.UseVisualStyleBackColor = true;
            // 
            // nud1
            // 
            nud1.Font = new Font("メイリオ", 12F);
            nud1.Location = new Point(130, 1);
            nud1.Margin = new Padding(1);
            nud1.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nud1.Name = "nud1";
            nud1.Size = new Size(57, 31);
            nud1.TabIndex = 1;
            nud1.TextAlign = HorizontalAlignment.Right;
            nud1.Value = new decimal(new int[] { 127, 0, 0, 0 });
            // 
            // cmbDevice
            // 
            cmbDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDevice.FlatStyle = FlatStyle.Flat;
            cmbDevice.Font = new Font("メイリオ", 12F);
            cmbDevice.FormattingEnabled = true;
            cmbDevice.Location = new Point(21, 0);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new Size(105, 32);
            cmbDevice.TabIndex = 2;
            // 
            // nud2
            // 
            nud2.Font = new Font("メイリオ", 12F);
            nud2.Location = new Point(189, 1);
            nud2.Margin = new Padding(1);
            nud2.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nud2.Name = "nud2";
            nud2.Size = new Size(57, 31);
            nud2.TabIndex = 3;
            nud2.TextAlign = HorizontalAlignment.Right;
            // 
            // nud3
            // 
            nud3.Font = new Font("メイリオ", 12F);
            nud3.Location = new Point(248, 1);
            nud3.Margin = new Padding(1);
            nud3.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nud3.Name = "nud3";
            nud3.Size = new Size(57, 31);
            nud3.TabIndex = 4;
            nud3.TextAlign = HorizontalAlignment.Right;
            // 
            // nud4
            // 
            nud4.Font = new Font("メイリオ", 12F);
            nud4.Location = new Point(307, 1);
            nud4.Margin = new Padding(1);
            nud4.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nud4.Name = "nud4";
            nud4.Size = new Size(57, 31);
            nud4.TabIndex = 5;
            nud4.TextAlign = HorizontalAlignment.Right;
            // 
            // nudPort
            // 
            nudPort.Font = new Font("メイリオ", 12F);
            nudPort.Location = new Point(377, 1);
            nudPort.Margin = new Padding(1);
            nudPort.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudPort.Name = "nudPort";
            nudPort.Size = new Size(79, 31);
            nudPort.TabIndex = 6;
            nudPort.TextAlign = HorizontalAlignment.Right;
            nudPort.Value = new decimal(new int[] { 5025, 0, 0, 0 });
            // 
            // UcServerConfig
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(nudPort);
            Controls.Add(nud4);
            Controls.Add(nud3);
            Controls.Add(nud2);
            Controls.Add(cmbDevice);
            Controls.Add(nud1);
            Controls.Add(chkUse);
            Name = "UcServerConfig";
            Size = new Size(458, 33);
            ((System.ComponentModel.ISupportInitialize)nud1).EndInit();
            ((System.ComponentModel.ISupportInitialize)nud2).EndInit();
            ((System.ComponentModel.ISupportInitialize)nud3).EndInit();
            ((System.ComponentModel.ISupportInitialize)nud4).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPort).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox chkUse;
        private NumericUpDown nud1;
        private ComboBox cmbDevice;
        private NumericUpDown nud2;
        private NumericUpDown nud3;
        private NumericUpDown nud4;
        private NumericUpDown nudPort;
    }
}
