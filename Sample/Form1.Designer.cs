namespace Sample
{
    partial class Form1
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
            rdbSerial = new RadioButton();
            pnlMenu = new Panel();
            pnlMain = new Panel();
            pnlMenu.SuspendLayout();
            SuspendLayout();
            // 
            // rdbSerial
            // 
            rdbSerial.Appearance = Appearance.Button;
            rdbSerial.Checked = true;
            rdbSerial.FlatAppearance.CheckedBackColor = Color.FromArgb(192, 255, 255);
            rdbSerial.FlatStyle = FlatStyle.Flat;
            rdbSerial.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            rdbSerial.Location = new Point(2, 1);
            rdbSerial.Name = "rdbSerial";
            rdbSerial.Size = new Size(102, 46);
            rdbSerial.TabIndex = 0;
            rdbSerial.TabStop = true;
            rdbSerial.Text = "シリアル";
            rdbSerial.TextAlign = ContentAlignment.MiddleCenter;
            rdbSerial.UseVisualStyleBackColor = true;
            rdbSerial.CheckedChanged += rdbSerail_CheckedChanged;
            // 
            // pnlMenu
            // 
            pnlMenu.BackColor = Color.Gainsboro;
            pnlMenu.Controls.Add(rdbSerial);
            pnlMenu.Dock = DockStyle.Top;
            pnlMenu.Location = new Point(0, 0);
            pnlMenu.Name = "pnlMenu";
            pnlMenu.Size = new Size(762, 47);
            pnlMenu.TabIndex = 1;
            // 
            // pnlMain
            // 
            pnlMain.Dock = DockStyle.Bottom;
            pnlMain.Location = new Point(0, 47);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(762, 446);
            pnlMain.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(762, 493);
            Controls.Add(pnlMain);
            Controls.Add(pnlMenu);
            Name = "Form1";
            Text = "Form1";
            Shown += Form1_Shown;
            pnlMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private RadioButton rdbSerial;
        private Panel pnlMenu;
        private Panel pnlMain;
    }
}
