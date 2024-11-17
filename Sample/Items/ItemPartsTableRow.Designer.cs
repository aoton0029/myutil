namespace Sample.Items
{
    partial class ItemPartsTableRow
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
            label6 = new Label();
            label5 = new Label();
            btnUp = new Button();
            txtNumber = new TextBox();
            btnDown = new Button();
            panel1 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label11
            // 
            label11.BorderStyle = BorderStyle.FixedSingle;
            label11.Font = new Font("メイリオ", 11.25F);
            label11.ForeColor = Color.Black;
            label11.Location = new Point(722, 0);
            label11.Margin = new Padding(0);
            label11.Name = "label11";
            label11.Size = new Size(144, 36);
            label11.TabIndex = 38;
            label11.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            label10.BorderStyle = BorderStyle.FixedSingle;
            label10.Font = new Font("メイリオ", 11.25F);
            label10.ForeColor = Color.Black;
            label10.Location = new Point(423, 0);
            label10.Margin = new Padding(0);
            label10.Name = "label10";
            label10.Size = new Size(105, 36);
            label10.TabIndex = 37;
            label10.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            label6.BorderStyle = BorderStyle.FixedSingle;
            label6.Font = new Font("メイリオ", 11.25F);
            label6.ForeColor = Color.Black;
            label6.Location = new Point(0, 0);
            label6.Margin = new Padding(0);
            label6.Name = "label6";
            label6.Size = new Size(109, 36);
            label6.TabIndex = 35;
            label6.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            label5.BorderStyle = BorderStyle.FixedSingle;
            label5.Font = new Font("メイリオ", 11.25F);
            label5.ForeColor = Color.Black;
            label5.Location = new Point(108, 0);
            label5.Margin = new Padding(0);
            label5.Name = "label5";
            label5.Size = new Size(315, 36);
            label5.TabIndex = 34;
            label5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnUp
            // 
            btnUp.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnUp.Location = new Point(151, -1);
            btnUp.Name = "btnUp";
            btnUp.Size = new Size(37, 36);
            btnUp.TabIndex = 41;
            btnUp.Text = "▲";
            btnUp.UseVisualStyleBackColor = true;
            // 
            // txtNumber
            // 
            txtNumber.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            txtNumber.Location = new Point(43, 1);
            txtNumber.Margin = new Padding(0);
            txtNumber.Name = "txtNumber";
            txtNumber.Size = new Size(105, 31);
            txtNumber.TabIndex = 40;
            // 
            // btnDown
            // 
            btnDown.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnDown.Location = new Point(4, -1);
            btnDown.Name = "btnDown";
            btnDown.Size = new Size(37, 36);
            btnDown.TabIndex = 39;
            btnDown.Text = "▼";
            btnDown.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(btnDown);
            panel1.Controls.Add(btnUp);
            panel1.Controls.Add(txtNumber);
            panel1.Location = new Point(528, 0);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(194, 36);
            panel1.TabIndex = 42;
            // 
            // ItemPartsTableRow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel1);
            Controls.Add(label11);
            Controls.Add(label10);
            Controls.Add(label6);
            Controls.Add(label5);
            Margin = new Padding(0);
            Name = "ItemPartsTableRow";
            Size = new Size(866, 36);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label label11;
        private Label label10;
        private Label label6;
        private Label label5;
        private Button btnUp;
        private TextBox txtNumber;
        private Button btnDown;
        private Panel panel1;
    }
}
