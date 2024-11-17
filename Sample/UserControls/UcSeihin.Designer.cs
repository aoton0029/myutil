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
            label5 = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            button1 = new Button();
            button2 = new Button();
            itemPartsTable1 = new Items.ItemPartsTable();
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
            // label1
            // 
            label1.BorderStyle = BorderStyle.FixedSingle;
            label1.Font = new Font("メイリオ", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label1.Location = new Point(14, 55);
            label1.Margin = new Padding(0);
            label1.Name = "label1";
            label1.Size = new Size(118, 37);
            label1.TabIndex = 11;
            label1.Text = "製品名";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.BorderStyle = BorderStyle.FixedSingle;
            label2.Font = new Font("メイリオ", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label2.Location = new Point(132, 55);
            label2.Margin = new Padding(0);
            label2.Name = "label2";
            label2.Size = new Size(639, 37);
            label2.TabIndex = 12;
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.BorderStyle = BorderStyle.FixedSingle;
            label3.Font = new Font("メイリオ", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label3.Location = new Point(132, 92);
            label3.Margin = new Padding(0);
            label3.Name = "label3";
            label3.Size = new Size(639, 36);
            label3.TabIndex = 14;
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            label4.BorderStyle = BorderStyle.FixedSingle;
            label4.Font = new Font("メイリオ", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label4.Location = new Point(14, 92);
            label4.Margin = new Padding(0);
            label4.Name = "label4";
            label4.Size = new Size(118, 36);
            label4.TabIndex = 13;
            label4.Text = "オプション";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            button1.Location = new Point(774, 56);
            button1.Name = "button1";
            button1.Size = new Size(107, 37);
            button1.TabIndex = 15;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(774, 93);
            button2.Name = "button2";
            button2.Size = new Size(107, 36);
            button2.TabIndex = 16;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // itemPartsTable1
            // 
            itemPartsTable1.Location = new Point(14, 134);
            itemPartsTable1.Name = "itemPartsTable1";
            itemPartsTable1.Size = new Size(885, 561);
            itemPartsTable1.TabIndex = 17;
            // 
            // UcSeihin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(itemPartsTable1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(label3);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(label5);
            Name = "UcSeihin";
            Size = new Size(888, 704);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label5;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Button button1;
        private Button button2;
        private Items.ItemPartsTable itemPartsTable1;
    }
}
