namespace Sample.Items
{
    partial class ItemPartsTable
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
            label11 = new Label();
            label10 = new Label();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // label11
            // 
            label11.BackColor = Color.Gray;
            label11.BorderStyle = BorderStyle.FixedSingle;
            label11.Font = new Font("メイリオ", 11.25F);
            label11.ForeColor = Color.White;
            label11.Location = new Point(723, 0);
            label11.Margin = new Padding(0);
            label11.Name = "label11";
            label11.Size = new Size(144, 31);
            label11.TabIndex = 33;
            label11.Text = "価格";
            label11.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            label10.BackColor = Color.Gray;
            label10.BorderStyle = BorderStyle.FixedSingle;
            label10.Font = new Font("メイリオ", 11.25F);
            label10.ForeColor = Color.White;
            label10.Location = new Point(424, 0);
            label10.Margin = new Padding(0);
            label10.Name = "label10";
            label10.Size = new Size(105, 31);
            label10.TabIndex = 32;
            label10.Text = "単価";
            label10.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            label7.BackColor = Color.Gray;
            label7.BorderStyle = BorderStyle.FixedSingle;
            label7.Font = new Font("メイリオ", 11.25F);
            label7.ForeColor = Color.White;
            label7.Location = new Point(529, 0);
            label7.Margin = new Padding(0);
            label7.Name = "label7";
            label7.Size = new Size(194, 31);
            label7.TabIndex = 31;
            label7.Text = "数量";
            label7.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            label6.BackColor = Color.Gray;
            label6.BorderStyle = BorderStyle.FixedSingle;
            label6.Font = new Font("メイリオ", 11.25F);
            label6.ForeColor = Color.White;
            label6.Location = new Point(0, 0);
            label6.Margin = new Padding(0);
            label6.Name = "label6";
            label6.Size = new Size(109, 31);
            label6.TabIndex = 30;
            label6.Text = "P/N";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            label5.BackColor = Color.Gray;
            label5.BorderStyle = BorderStyle.FixedSingle;
            label5.Font = new Font("メイリオ", 11.25F);
            label5.ForeColor = Color.White;
            label5.Location = new Point(109, 0);
            label5.Margin = new Padding(0);
            label5.Name = "label5";
            label5.Size = new Size(315, 31);
            label5.TabIndex = 29;
            label5.Text = "正式品番";
            label5.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label4.BorderStyle = BorderStyle.FixedSingle;
            label4.Font = new Font("メイリオ", 14.25F);
            label4.Location = new Point(109, 524);
            label4.Name = "label4";
            label4.Size = new Size(758, 37);
            label4.TabIndex = 28;
            label4.Text = "\\1000000";
            label4.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label3.BorderStyle = BorderStyle.FixedSingle;
            label3.Font = new Font("メイリオ", 14.25F);
            label3.Location = new Point(0, 524);
            label3.Name = "label3";
            label3.Size = new Size(109, 37);
            label3.TabIndex = 27;
            label3.Text = "価格";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.Location = new Point(0, 31);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(885, 493);
            flowLayoutPanel1.TabIndex = 34;
            // 
            // ItemPartsTable
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(flowLayoutPanel1);
            Controls.Add(label11);
            Controls.Add(label10);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Name = "ItemPartsTable";
            Size = new Size(885, 561);
            ResumeLayout(false);
        }

        #endregion

        private Label label11;
        private Label label10;
        private Label label7;
        private Label label6;
        private Label label5;
        private Label label4;
        private Label label3;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}
