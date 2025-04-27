namespace SearchAppSample.Items
{
    partial class UcItemDataGrid
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
            dataGridView1 = new DataGridView();
            btnFirst = new Button();
            btnPrev = new Button();
            btnLast = new Button();
            btnNext = new Button();
            lblPageInfo = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            cmbPageSize = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            tableLayoutPanel1.SetColumnSpan(dataGridView1, 7);
            dataGridView1.Location = new Point(3, 3);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(976, 506);
            dataGridView1.TabIndex = 0;
            // 
            // btnFirst
            // 
            btnFirst.Dock = DockStyle.Fill;
            btnFirst.Font = new Font("メイリオ", 14.25F);
            btnFirst.Location = new Point(395, 515);
            btnFirst.Name = "btnFirst";
            btnFirst.Size = new Size(92, 51);
            btnFirst.TabIndex = 1;
            btnFirst.Text = "First";
            btnFirst.UseVisualStyleBackColor = true;
            btnFirst.Click += btnFirst_Click;
            // 
            // btnPrev
            // 
            btnPrev.Dock = DockStyle.Fill;
            btnPrev.Font = new Font("メイリオ", 14.25F);
            btnPrev.Location = new Point(493, 515);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(92, 51);
            btnPrev.TabIndex = 2;
            btnPrev.Text = "Prev";
            btnPrev.UseVisualStyleBackColor = true;
            btnPrev.Click += btnPrev_Click;
            // 
            // btnLast
            // 
            btnLast.Dock = DockStyle.Fill;
            btnLast.Font = new Font("メイリオ", 14.25F);
            btnLast.Location = new Point(885, 515);
            btnLast.Name = "btnLast";
            btnLast.Size = new Size(94, 51);
            btnLast.TabIndex = 4;
            btnLast.Text = "Last";
            btnLast.UseVisualStyleBackColor = true;
            btnLast.Click += btnLast_Click;
            // 
            // btnNext
            // 
            btnNext.Dock = DockStyle.Fill;
            btnNext.Font = new Font("メイリオ", 14.25F);
            btnNext.Location = new Point(787, 515);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(92, 51);
            btnNext.TabIndex = 3;
            btnNext.Text = "Next";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // lblPageInfo
            // 
            lblPageInfo.BorderStyle = BorderStyle.FixedSingle;
            lblPageInfo.Dock = DockStyle.Fill;
            lblPageInfo.Font = new Font("メイリオ", 14.25F);
            lblPageInfo.Location = new Point(591, 515);
            lblPageInfo.Margin = new Padding(3);
            lblPageInfo.Name = "lblPageInfo";
            lblPageInfo.Size = new Size(190, 51);
            lblPageInfo.TabIndex = 5;
            lblPageInfo.Text = "label1";
            lblPageInfo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 7;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.Controls.Add(dataGridView1, 0, 0);
            tableLayoutPanel1.Controls.Add(btnLast, 6, 1);
            tableLayoutPanel1.Controls.Add(lblPageInfo, 4, 1);
            tableLayoutPanel1.Controls.Add(btnNext, 5, 1);
            tableLayoutPanel1.Controls.Add(btnFirst, 2, 1);
            tableLayoutPanel1.Controls.Add(btnPrev, 3, 1);
            tableLayoutPanel1.Controls.Add(cmbPageSize, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 90F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.Size = new Size(982, 569);
            tableLayoutPanel1.TabIndex = 6;
            // 
            // cmbPageSize
            // 
            cmbPageSize.Dock = DockStyle.Fill;
            cmbPageSize.FormattingEnabled = true;
            cmbPageSize.Location = new Point(199, 515);
            cmbPageSize.Name = "cmbPageSize";
            cmbPageSize.Size = new Size(190, 23);
            cmbPageSize.TabIndex = 6;
            // 
            // UcItemDataGrid
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "UcItemDataGrid";
            Size = new Size(982, 569);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private Button btnFirst;
        private Button btnPrev;
        private Button btnLast;
        private Button btnNext;
        private Label lblPageInfo;
        private TableLayoutPanel tableLayoutPanel1;
        private ComboBox cmbPageSize;
    }
}
