namespace SearchAppSample.Pages
{
    partial class UcPageMain
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
            panel1 = new Panel();
            button6 = new Button();
            button5 = new Button();
            button4 = new Button();
            button3 = new Button();
            button2 = new Button();
            button1 = new Button();
            ucItemHeader1 = new SearchAppSample.Items.UcItemHeader();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Location = new Point(371, 82);
            panel1.Name = "panel1";
            panel1.Size = new Size(939, 654);
            panel1.TabIndex = 13;
            // 
            // button6
            // 
            button6.BackColor = Color.SteelBlue;
            button6.FlatStyle = FlatStyle.Flat;
            button6.Font = new Font("メイリオ", 21.75F);
            button6.ForeColor = Color.White;
            button6.Location = new Point(41, 632);
            button6.Name = "button6";
            button6.Size = new Size(299, 104);
            button6.TabIndex = 12;
            button6.Text = "▶ P/Nから検索";
            button6.UseVisualStyleBackColor = false;
            // 
            // button5
            // 
            button5.BackColor = Color.SteelBlue;
            button5.FlatStyle = FlatStyle.Flat;
            button5.Font = new Font("メイリオ", 21.75F);
            button5.ForeColor = Color.White;
            button5.Location = new Point(41, 522);
            button5.Name = "button5";
            button5.Size = new Size(299, 104);
            button5.TabIndex = 11;
            button5.Text = "▶ B/Nから検索";
            button5.UseVisualStyleBackColor = false;
            // 
            // button4
            // 
            button4.BackColor = Color.SteelBlue;
            button4.FlatStyle = FlatStyle.Flat;
            button4.Font = new Font("メイリオ", 21.75F);
            button4.ForeColor = Color.White;
            button4.Location = new Point(41, 412);
            button4.Name = "button4";
            button4.Size = new Size(299, 104);
            button4.TabIndex = 10;
            button4.Text = "▶ S/Nから検索";
            button4.UseVisualStyleBackColor = false;
            // 
            // button3
            // 
            button3.BackColor = Color.SteelBlue;
            button3.FlatStyle = FlatStyle.Flat;
            button3.Font = new Font("メイリオ", 21.75F);
            button3.ForeColor = Color.White;
            button3.Location = new Point(41, 302);
            button3.Name = "button3";
            button3.Size = new Size(299, 104);
            button3.TabIndex = 9;
            button3.Text = "▶ 型名から検索";
            button3.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            button2.BackColor = Color.SteelBlue;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("メイリオ", 21.75F);
            button2.ForeColor = Color.White;
            button2.Location = new Point(41, 192);
            button2.Name = "button2";
            button2.Size = new Size(299, 104);
            button2.TabIndex = 8;
            button2.Text = "▶ 注番から検索";
            button2.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            button1.BackColor = Color.SteelBlue;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("メイリオ", 21.75F);
            button1.ForeColor = Color.White;
            button1.Location = new Point(41, 82);
            button1.Name = "button1";
            button1.Size = new Size(299, 104);
            button1.TabIndex = 7;
            button1.Text = "▶ 工番から検索";
            button1.UseVisualStyleBackColor = false;
            // 
            // ucItemHeader1
            // 
            ucItemHeader1.BackColor = Color.FromArgb(43, 79, 107);
            ucItemHeader1.Dock = DockStyle.Top;
            ucItemHeader1.Location = new Point(0, 0);
            ucItemHeader1.Name = "ucItemHeader1";
            ucItemHeader1.Size = new Size(1350, 33);
            ucItemHeader1.TabIndex = 14;
            ucItemHeader1.Title = "Title";
            // 
            // UcPageMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(ucItemHeader1);
            Controls.Add(panel1);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "UcPageMain";
            Size = new Size(1350, 780);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button button6;
        private Button button5;
        private Button button4;
        private Button button3;
        private Button button2;
        private Button button1;
        private Items.UcItemHeader ucItemHeader1;
    }
}
