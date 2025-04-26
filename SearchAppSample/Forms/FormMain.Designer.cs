namespace SearchAppSample
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            button6 = new Button();
            panel1 = new Panel();
            SuspendLayout();
            // 
            // button1
            // 
            button1.BackColor = Color.SteelBlue;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("メイリオ", 21.75F);
            button1.ForeColor = Color.White;
            button1.Location = new Point(28, 26);
            button1.Name = "button1";
            button1.Size = new Size(299, 104);
            button1.TabIndex = 0;
            button1.Text = "▶ 工番から検索";
            button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            button2.BackColor = Color.SteelBlue;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("メイリオ", 21.75F);
            button2.ForeColor = Color.White;
            button2.Location = new Point(28, 136);
            button2.Name = "button2";
            button2.Size = new Size(299, 104);
            button2.TabIndex = 1;
            button2.Text = "▶ 注番から検索";
            button2.UseVisualStyleBackColor = false;
            // 
            // button3
            // 
            button3.BackColor = Color.SteelBlue;
            button3.FlatStyle = FlatStyle.Flat;
            button3.Font = new Font("メイリオ", 21.75F);
            button3.ForeColor = Color.White;
            button3.Location = new Point(28, 246);
            button3.Name = "button3";
            button3.Size = new Size(299, 104);
            button3.TabIndex = 2;
            button3.Text = "▶ 型名から検索";
            button3.UseVisualStyleBackColor = false;
            // 
            // button4
            // 
            button4.BackColor = Color.SteelBlue;
            button4.FlatStyle = FlatStyle.Flat;
            button4.Font = new Font("メイリオ", 21.75F);
            button4.ForeColor = Color.White;
            button4.Location = new Point(28, 356);
            button4.Name = "button4";
            button4.Size = new Size(299, 104);
            button4.TabIndex = 3;
            button4.Text = "▶ S/Nから検索";
            button4.UseVisualStyleBackColor = false;
            // 
            // button5
            // 
            button5.BackColor = Color.SteelBlue;
            button5.FlatStyle = FlatStyle.Flat;
            button5.Font = new Font("メイリオ", 21.75F);
            button5.ForeColor = Color.White;
            button5.Location = new Point(28, 466);
            button5.Name = "button5";
            button5.Size = new Size(299, 104);
            button5.TabIndex = 4;
            button5.Text = "▶ B/Nから検索";
            button5.UseVisualStyleBackColor = false;
            // 
            // button6
            // 
            button6.BackColor = Color.SteelBlue;
            button6.FlatStyle = FlatStyle.Flat;
            button6.Font = new Font("メイリオ", 21.75F);
            button6.ForeColor = Color.White;
            button6.Location = new Point(28, 576);
            button6.Name = "button6";
            button6.Size = new Size(299, 104);
            button6.TabIndex = 5;
            button6.Text = "▶ P/Nから検索";
            button6.UseVisualStyleBackColor = false;
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Location = new Point(363, 26);
            panel1.Name = "panel1";
            panel1.Size = new Size(718, 654);
            panel1.TabIndex = 6;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1128, 709);
            Controls.Add(panel1);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "FormMain";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Panel panel1;
    }
}
