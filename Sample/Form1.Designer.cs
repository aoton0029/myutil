namespace Sample
{
    partial class Form1
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
            panel1 = new Panel();
            btnOption = new Button();
            btnParts = new Button();
            btnSeihin = new Button();
            btnHome = new Button();
            panel2 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(46, 131, 190);
            panel1.Controls.Add(btnOption);
            panel1.Controls.Add(btnHome);
            panel1.Controls.Add(btnSeihin);
            panel1.Controls.Add(btnParts);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1384, 46);
            panel1.TabIndex = 0;
            // 
            // btnOption
            // 
            btnOption.BackColor = Color.FromArgb(46, 131, 190);
            btnOption.FlatAppearance.BorderSize = 0;
            btnOption.FlatStyle = FlatStyle.Flat;
            btnOption.Font = new Font("メイリオ", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnOption.ForeColor = Color.White;
            btnOption.Location = new Point(549, 0);
            btnOption.Margin = new Padding(0);
            btnOption.Name = "btnOption";
            btnOption.Size = new Size(183, 46);
            btnOption.TabIndex = 2;
            btnOption.Text = "オプション管理";
            btnOption.UseVisualStyleBackColor = false;
            btnOption.Click += btnOption_Click;
            // 
            // btnParts
            // 
            btnParts.BackColor = Color.FromArgb(46, 131, 190);
            btnParts.FlatAppearance.BorderSize = 0;
            btnParts.FlatStyle = FlatStyle.Flat;
            btnParts.Font = new Font("メイリオ", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnParts.ForeColor = Color.White;
            btnParts.Location = new Point(366, 0);
            btnParts.Margin = new Padding(0);
            btnParts.Name = "btnParts";
            btnParts.Size = new Size(183, 46);
            btnParts.TabIndex = 1;
            btnParts.Text = "製品管理";
            btnParts.UseVisualStyleBackColor = false;
            btnParts.Click += btnParts_Click;
            // 
            // btnSeihin
            // 
            btnSeihin.BackColor = Color.FromArgb(46, 131, 190);
            btnSeihin.FlatAppearance.BorderSize = 0;
            btnSeihin.FlatStyle = FlatStyle.Flat;
            btnSeihin.Font = new Font("メイリオ", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnSeihin.ForeColor = Color.White;
            btnSeihin.Location = new Point(183, 0);
            btnSeihin.Margin = new Padding(0);
            btnSeihin.Name = "btnSeihin";
            btnSeihin.Size = new Size(183, 46);
            btnSeihin.TabIndex = 3;
            btnSeihin.Text = "製品一覧";
            btnSeihin.UseVisualStyleBackColor = false;
            btnSeihin.Click += btnSeihin_Click;
            // 
            // btnHome
            // 
            btnHome.BackColor = Color.FromArgb(20, 96, 146);
            btnHome.FlatAppearance.BorderSize = 0;
            btnHome.FlatStyle = FlatStyle.Flat;
            btnHome.Font = new Font("メイリオ", 14.25F, FontStyle.Underline, GraphicsUnit.Point, 128);
            btnHome.ForeColor = Color.White;
            btnHome.Location = new Point(0, 0);
            btnHome.Margin = new Padding(0);
            btnHome.Name = "btnHome";
            btnHome.Size = new Size(183, 46);
            btnHome.TabIndex = 0;
            btnHome.Text = "ホーム";
            btnHome.UseVisualStyleBackColor = false;
            btnHome.Click += btnHome_Click;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel2.Location = new Point(0, 48);
            panel2.Name = "panel2";
            panel2.Size = new Size(1384, 712);
            panel2.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1384, 761);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btnHome;
        private Panel panel2;
        private Button btnParts;
        private Button btnOption;
        private Button btnSeihin;
    }
}