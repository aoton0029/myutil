namespace UtilityLib.Pagenations
{
    partial class UcPagenation
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
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnPrevious = new Button();
            btnNext = new Button();
            lblPageInfo = new Label();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Location = new Point(106, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(371, 70);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // btnPrevious
            // 
            btnPrevious.Location = new Point(25, 28);
            btnPrevious.Name = "btnPrevious";
            btnPrevious.Size = new Size(75, 23);
            btnPrevious.TabIndex = 1;
            btnPrevious.Text = "button1";
            btnPrevious.UseVisualStyleBackColor = true;
            // 
            // btnNext
            // 
            btnNext.Location = new Point(483, 28);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(75, 23);
            btnNext.TabIndex = 2;
            btnNext.Text = "button2";
            btnNext.UseVisualStyleBackColor = true;
            // 
            // lblPageInfo
            // 
            lblPageInfo.AutoSize = true;
            lblPageInfo.Location = new Point(25, 10);
            lblPageInfo.Name = "lblPageInfo";
            lblPageInfo.Size = new Size(38, 15);
            lblPageInfo.TabIndex = 0;
            lblPageInfo.Text = "label1";
            // 
            // UcPagenation
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblPageInfo);
            Controls.Add(btnNext);
            Controls.Add(btnPrevious);
            Controls.Add(flowLayoutPanel1);
            Name = "UcPagenation";
            Size = new Size(584, 76);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnPrevious;
        private Button btnNext;
        private Label lblPageInfo;
    }
}
