namespace SampleTcpServer
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
            btnStart = new Button();
            btnStop = new Button();
            ucServerConfig1 = new UcServerConfig();
            ucServerConfig2 = new UcServerConfig();
            ucServerConfig3 = new UcServerConfig();
            ucServerConfig4 = new UcServerConfig();
            listBox1 = new ListBox();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnStart.Location = new Point(12, 12);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(107, 66);
            btnStart.TabIndex = 0;
            btnStart.Text = "起動";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // btnStop
            // 
            btnStop.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnStop.Location = new Point(125, 12);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(107, 66);
            btnStop.TabIndex = 1;
            btnStop.Text = "停止";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // ucServerConfig1
            // 
            ucServerConfig1.IP1 = 127;
            ucServerConfig1.IP2 = 0;
            ucServerConfig1.IP3 = 0;
            ucServerConfig1.IP4 = 0;
            ucServerConfig1.IsUse = false;
            ucServerConfig1.Location = new Point(9, 84);
            ucServerConfig1.Margin = new Padding(0);
            ucServerConfig1.Name = "ucServerConfig1";
            ucServerConfig1.Size = new Size(458, 33);
            ucServerConfig1.TabIndex = 2;
            // 
            // ucServerConfig2
            // 
            ucServerConfig2.IP1 = 127;
            ucServerConfig2.IP2 = 0;
            ucServerConfig2.IP3 = 0;
            ucServerConfig2.IP4 = 0;
            ucServerConfig2.IsUse = false;
            ucServerConfig2.Location = new Point(9, 117);
            ucServerConfig2.Margin = new Padding(0);
            ucServerConfig2.Name = "ucServerConfig2";
            ucServerConfig2.Size = new Size(458, 33);
            ucServerConfig2.TabIndex = 3;
            // 
            // ucServerConfig3
            // 
            ucServerConfig3.IP1 = 127;
            ucServerConfig3.IP2 = 0;
            ucServerConfig3.IP3 = 0;
            ucServerConfig3.IP4 = 0;
            ucServerConfig3.IsUse = false;
            ucServerConfig3.Location = new Point(9, 150);
            ucServerConfig3.Margin = new Padding(0);
            ucServerConfig3.Name = "ucServerConfig3";
            ucServerConfig3.Size = new Size(458, 33);
            ucServerConfig3.TabIndex = 4;
            // 
            // ucServerConfig4
            // 
            ucServerConfig4.IP1 = 127;
            ucServerConfig4.IP2 = 0;
            ucServerConfig4.IP3 = 0;
            ucServerConfig4.IP4 = 0;
            ucServerConfig4.IsUse = false;
            ucServerConfig4.Location = new Point(9, 183);
            ucServerConfig4.Margin = new Padding(0);
            ucServerConfig4.Name = "ucServerConfig4";
            ucServerConfig4.Size = new Size(458, 33);
            ucServerConfig4.TabIndex = 5;
            // 
            // listBox1
            // 
            listBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBox1.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 24;
            listBox1.Location = new Point(483, 12);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(844, 700);
            listBox1.TabIndex = 6;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1339, 720);
            Controls.Add(listBox1);
            Controls.Add(ucServerConfig4);
            Controls.Add(ucServerConfig3);
            Controls.Add(ucServerConfig2);
            Controls.Add(ucServerConfig1);
            Controls.Add(btnStop);
            Controls.Add(btnStart);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button btnStart;
        private Button btnStop;
        private UcServerConfig ucServerConfig1;
        private UcServerConfig ucServerConfig2;
        private UcServerConfig ucServerConfig3;
        private UcServerConfig ucServerConfig4;
        private ListBox listBox1;
    }
}
