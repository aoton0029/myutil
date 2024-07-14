namespace Sample
{
    partial class FormDualList
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
            lbLeft = new ListBox();
            lbRight = new ListBox();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            btnLeftToRight = new Button();
            btnRightToLeft = new Button();
            SuspendLayout();
            // 
            // lbLeft
            // 
            lbLeft.FormattingEnabled = true;
            lbLeft.ItemHeight = 15;
            lbLeft.Location = new Point(12, 58);
            lbLeft.Name = "lbLeft";
            lbLeft.Size = new Size(290, 424);
            lbLeft.TabIndex = 0;
            // 
            // lbRight
            // 
            lbRight.FormattingEnabled = true;
            lbRight.ItemHeight = 15;
            lbRight.Location = new Point(498, 58);
            lbRight.Name = "lbRight";
            lbRight.Size = new Size(290, 424);
            lbRight.TabIndex = 1;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 29);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(290, 23);
            textBox1.TabIndex = 2;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(498, 29);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(290, 23);
            textBox2.TabIndex = 3;
            // 
            // btnLeftToRight
            // 
            btnLeftToRight.Font = new Font("Yu Gothic UI", 24F);
            btnLeftToRight.Location = new Point(339, 153);
            btnLeftToRight.Name = "btnLeftToRight";
            btnLeftToRight.Size = new Size(120, 99);
            btnLeftToRight.TabIndex = 4;
            btnLeftToRight.Text = "◀";
            btnLeftToRight.UseVisualStyleBackColor = true;
            // 
            // btnRightToLeft
            // 
            btnRightToLeft.Font = new Font("Yu Gothic UI", 24F);
            btnRightToLeft.Location = new Point(339, 258);
            btnRightToLeft.Name = "btnRightToLeft";
            btnRightToLeft.Size = new Size(120, 99);
            btnRightToLeft.TabIndex = 5;
            btnRightToLeft.Text = "▶";
            btnRightToLeft.UseVisualStyleBackColor = true;
            // 
            // FormDualList
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 492);
            Controls.Add(btnRightToLeft);
            Controls.Add(btnLeftToRight);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(lbRight);
            Controls.Add(lbLeft);
            Name = "FormDualList";
            Text = "FormDualList";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox lbLeft;
        private ListBox lbRight;
        private TextBox textBox1;
        private TextBox textBox2;
        private Button btnLeftToRight;
        private Button btnRightToLeft;
    }
}