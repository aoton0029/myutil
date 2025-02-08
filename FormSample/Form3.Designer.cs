namespace FormSample
{
    partial class Form3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            panel1 = new Panel();
            SuspendLayout();
            // 
            // button1
            // 
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("メイリオ", 14.25F);
            button1.ForeColor = Color.DodgerBlue;
            button1.Location = new Point(5, 5);
            button1.Margin = new Padding(0);
            button1.Name = "button1";
            button1.Size = new Size(127, 40);
            button1.TabIndex = 0;
            button1.Text = "検索条件";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("メイリオ", 14.25F);
            button2.ForeColor = Color.DodgerBlue;
            button2.Location = new Point(132, 5);
            button2.Margin = new Padding(0);
            button2.Name = "button2";
            button2.Size = new Size(127, 40);
            button2.TabIndex = 1;
            button2.Text = "検索履歴";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.FlatStyle = FlatStyle.Flat;
            button3.Font = new Font("メイリオ", 14.25F);
            button3.ForeColor = Color.DodgerBlue;
            button3.Location = new Point(259, 5);
            button3.Margin = new Padding(0);
            button3.Name = "button3";
            button3.Size = new Size(127, 40);
            button3.TabIndex = 2;
            button3.Text = "検索履歴";
            button3.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Location = new Point(5, 45);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1763, 145);
            panel1.TabIndex = 3;
            // 
            // Form3
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1784, 961);
            Controls.Add(panel1);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "Form3";
            Text = "Form3";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button3;
        private Panel panel1;
    }
}