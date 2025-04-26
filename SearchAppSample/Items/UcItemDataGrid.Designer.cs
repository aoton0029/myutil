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
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(3, 3);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(638, 464);
            dataGridView1.TabIndex = 0;
            // 
            // btnFirst
            // 
            btnFirst.Anchor = AnchorStyles.Bottom;
            btnFirst.Font = new Font("メイリオ", 14.25F);
            btnFirst.Location = new Point(3, 473);
            btnFirst.Name = "btnFirst";
            btnFirst.Size = new Size(117, 51);
            btnFirst.TabIndex = 1;
            btnFirst.Text = "First";
            btnFirst.UseVisualStyleBackColor = true;
            // 
            // btnPrev
            // 
            btnPrev.Anchor = AnchorStyles.Bottom;
            btnPrev.Font = new Font("メイリオ", 14.25F);
            btnPrev.Location = new Point(126, 473);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(117, 51);
            btnPrev.TabIndex = 2;
            btnPrev.Text = "Prev";
            btnPrev.UseVisualStyleBackColor = true;
            // 
            // btnLast
            // 
            btnLast.Anchor = AnchorStyles.Bottom;
            btnLast.Font = new Font("メイリオ", 14.25F);
            btnLast.Location = new Point(525, 473);
            btnLast.Name = "btnLast";
            btnLast.Size = new Size(117, 51);
            btnLast.TabIndex = 4;
            btnLast.Text = "Last";
            btnLast.UseVisualStyleBackColor = true;
            // 
            // btnNext
            // 
            btnNext.Anchor = AnchorStyles.Bottom;
            btnNext.Font = new Font("メイリオ", 14.25F);
            btnNext.Location = new Point(402, 473);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(117, 51);
            btnNext.TabIndex = 3;
            btnNext.Text = "Next";
            btnNext.UseVisualStyleBackColor = true;
            // 
            // lblPageInfo
            // 
            lblPageInfo.Anchor = AnchorStyles.Bottom;
            lblPageInfo.BorderStyle = BorderStyle.FixedSingle;
            lblPageInfo.Font = new Font("メイリオ", 14.25F);
            lblPageInfo.Location = new Point(249, 473);
            lblPageInfo.Name = "lblPageInfo";
            lblPageInfo.Size = new Size(147, 51);
            lblPageInfo.TabIndex = 5;
            lblPageInfo.Text = "label1";
            lblPageInfo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // UcItemDataGrid
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblPageInfo);
            Controls.Add(btnLast);
            Controls.Add(btnNext);
            Controls.Add(btnPrev);
            Controls.Add(btnFirst);
            Controls.Add(dataGridView1);
            Name = "UcItemDataGrid";
            Size = new Size(644, 527);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private Button btnFirst;
        private Button btnPrev;
        private Button btnLast;
        private Button btnNext;
        private Label lblPageInfo;
    }
}
