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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            listBoxFiles = new ListBox();
            button1 = new Button();
            lblDate = new Label();
            button4 = new Button();
            label1 = new Label();
            axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
            btnPlay = new Button();
            button3 = new Button();
            ((System.ComponentModel.ISupportInitialize)axWindowsMediaPlayer1).BeginInit();
            SuspendLayout();
            // 
            // listBoxFiles
            // 
            listBoxFiles.FormattingEnabled = true;
            listBoxFiles.ItemHeight = 15;
            listBoxFiles.Location = new Point(12, 15);
            listBoxFiles.Name = "listBoxFiles";
            listBoxFiles.Size = new Size(623, 454);
            listBoxFiles.TabIndex = 0;
            // 
            // button1
            // 
            button1.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            button1.Location = new Point(641, 12);
            button1.Name = "button1";
            button1.Size = new Size(268, 42);
            button1.TabIndex = 1;
            button1.Text = "リスト更新";
            button1.UseVisualStyleBackColor = true;
            // 
            // lblDate
            // 
            lblDate.BackColor = Color.White;
            lblDate.BorderStyle = BorderStyle.FixedSingle;
            lblDate.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            lblDate.Location = new Point(641, 338);
            lblDate.Name = "lblDate";
            lblDate.Size = new Size(268, 34);
            lblDate.TabIndex = 6;
            lblDate.Text = "label2";
            lblDate.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // button4
            // 
            button4.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            button4.Location = new Point(641, 414);
            button4.Name = "button4";
            button4.Size = new Size(268, 55);
            button4.TabIndex = 7;
            button4.Text = "アップロード";
            button4.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.BackColor = Color.White;
            label1.BorderStyle = BorderStyle.FixedSingle;
            label1.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label1.Location = new Point(641, 300);
            label1.Name = "label1";
            label1.Size = new Size(268, 34);
            label1.TabIndex = 8;
            label1.Text = "label2";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // axWindowsMediaPlayer1
            // 
            axWindowsMediaPlayer1.Enabled = true;
            axWindowsMediaPlayer1.Location = new Point(641, 60);
            axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            axWindowsMediaPlayer1.OcxState = (AxHost.State)resources.GetObject("axWindowsMediaPlayer1.OcxState");
            axWindowsMediaPlayer1.Size = new Size(268, 237);
            axWindowsMediaPlayer1.TabIndex = 9;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(641, 375);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(90, 33);
            btnPlay.TabIndex = 10;
            btnPlay.Text = "PLAY";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            // 
            // button3
            // 
            button3.Location = new Point(801, 375);
            button3.Name = "button3";
            button3.Size = new Size(90, 33);
            button3.TabIndex = 11;
            button3.Text = "button3";
            button3.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(921, 546);
            Controls.Add(button3);
            Controls.Add(btnPlay);
            Controls.Add(axWindowsMediaPlayer1);
            Controls.Add(label1);
            Controls.Add(button4);
            Controls.Add(lblDate);
            Controls.Add(button1);
            Controls.Add(listBoxFiles);
            Name = "Form1";
            Text = "Form1";
            FormClosing += Form1_FormClosing;
            ((System.ComponentModel.ISupportInitialize)axWindowsMediaPlayer1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBoxFiles;
        private Button button1;
        private Label lblDate;
        private Button button4;
        private Label label1;
        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
        private Button btnPlay;
        private Button button3;
    }
}
