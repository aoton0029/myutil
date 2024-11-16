namespace Sample.UserControls
{
    partial class UcSeihin
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            label5 = new Label();
            dataGridView1 = new DataGridView();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            button1 = new Button();
            button2 = new Button();
            dataGridView2 = new DataGridView();
            ColumnPN = new DataGridViewTextBoxColumn();
            ColumnSeihinName = new DataGridViewTextBoxColumn();
            ColumnTanka = new DataGridViewTextBoxColumn();
            ColumnPrice = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("メイリオ", 18F, FontStyle.Bold, GraphicsUnit.Point, 128);
            label5.Location = new Point(5, 6);
            label5.Name = "label5";
            label5.Size = new Size(111, 36);
            label5.TabIndex = 9;
            label5.Text = "製品管理";
            // 
            // dataGridView1
            // 
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { ColumnPN, ColumnSeihinName, ColumnTanka, ColumnPrice });
            dataGridView1.Location = new Point(14, 183);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 20;
            dataGridView1.RowTemplate.Height = 30;
            dataGridView1.Size = new Size(751, 537);
            dataGridView1.TabIndex = 10;
            // 
            // label1
            // 
            label1.BorderStyle = BorderStyle.FixedSingle;
            label1.Font = new Font("メイリオ", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label1.Location = new Point(14, 55);
            label1.Margin = new Padding(0);
            label1.Name = "label1";
            label1.Size = new Size(118, 64);
            label1.TabIndex = 11;
            label1.Text = "製品名";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.BorderStyle = BorderStyle.FixedSingle;
            label2.Font = new Font("メイリオ", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label2.Location = new Point(132, 55);
            label2.Margin = new Padding(0, 0, 0, 0);
            label2.Name = "label2";
            label2.Size = new Size(523, 64);
            label2.TabIndex = 12;
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.BorderStyle = BorderStyle.FixedSingle;
            label3.Font = new Font("メイリオ", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label3.Location = new Point(132, 119);
            label3.Margin = new Padding(0);
            label3.Name = "label3";
            label3.Size = new Size(523, 64);
            label3.TabIndex = 14;
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            label4.BorderStyle = BorderStyle.FixedSingle;
            label4.Font = new Font("メイリオ", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label4.Location = new Point(14, 119);
            label4.Margin = new Padding(0);
            label4.Name = "label4";
            label4.Size = new Size(118, 64);
            label4.TabIndex = 13;
            label4.Text = "オプション";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            button1.Location = new Point(658, 56);
            button1.Name = "button1";
            button1.Size = new Size(107, 64);
            button1.TabIndex = 15;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(658, 121);
            button2.Name = "button2";
            button2.Size = new Size(107, 63);
            button2.TabIndex = 16;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // dataGridView2
            // 
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Control;
            dataGridViewCellStyle2.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dataGridView2.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1 });
            dataGridView2.Location = new Point(771, 56);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.RowHeadersWidth = 20;
            dataGridView2.RowTemplate.Height = 30;
            dataGridView2.Size = new Size(387, 664);
            dataGridView2.TabIndex = 17;
            // 
            // ColumnPN
            // 
            ColumnPN.HeaderText = "P/N";
            ColumnPN.Name = "ColumnPN";
            // 
            // ColumnSeihinName
            // 
            ColumnSeihinName.HeaderText = "正式品番";
            ColumnSeihinName.Name = "ColumnSeihinName";
            ColumnSeihinName.Width = 400;
            // 
            // ColumnTanka
            // 
            ColumnTanka.HeaderText = "単価";
            ColumnTanka.Name = "ColumnTanka";
            // 
            // ColumnPrice
            // 
            ColumnPrice.HeaderText = "価格";
            ColumnPrice.Name = "ColumnPrice";
            ColumnPrice.Width = 120;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "製品名";
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.Width = 300;
            // 
            // UcSeihin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(dataGridView2);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(label3);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(dataGridView1);
            Controls.Add(label5);
            Name = "UcSeihin";
            Size = new Size(1161, 732);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label5;
        private DataGridView dataGridView1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Button button1;
        private Button button2;
        private DataGridViewTextBoxColumn ColumnPN;
        private DataGridViewTextBoxColumn ColumnSeihinName;
        private DataGridViewTextBoxColumn ColumnTanka;
        private DataGridViewTextBoxColumn ColumnPrice;
        private DataGridView dataGridView2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    }
}
