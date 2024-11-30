namespace AsyncSample
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
            listBox1 = new ListBox();
            btnAwait = new Button();
            btnWait = new Button();
            btnAsyncVoid = new Button();
            btnTimer = new Button();
            btnException = new Button();
            btnContinueWith = new Button();
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(12, 12);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(517, 424);
            listBox1.TabIndex = 0;
            // 
            // btnAwait
            // 
            btnAwait.Location = new Point(561, 12);
            btnAwait.Name = "btnAwait";
            btnAwait.Size = new Size(168, 44);
            btnAwait.TabIndex = 1;
            btnAwait.Text = "await";
            btnAwait.UseVisualStyleBackColor = true;
            btnAwait.Click += btnAwait_Click;
            // 
            // btnWait
            // 
            btnWait.Location = new Point(561, 62);
            btnWait.Name = "btnWait";
            btnWait.Size = new Size(168, 44);
            btnWait.TabIndex = 2;
            btnWait.Text = "wait";
            btnWait.UseVisualStyleBackColor = true;
            btnWait.Click += btnWait_Click;
            // 
            // btnAsyncVoid
            // 
            btnAsyncVoid.Location = new Point(561, 112);
            btnAsyncVoid.Name = "btnAsyncVoid";
            btnAsyncVoid.Size = new Size(168, 44);
            btnAsyncVoid.TabIndex = 3;
            btnAsyncVoid.Text = "async void";
            btnAsyncVoid.UseVisualStyleBackColor = true;
            btnAsyncVoid.Click += btnAsyncVoid_ClickAsync;
            // 
            // btnTimer
            // 
            btnTimer.Location = new Point(561, 392);
            btnTimer.Name = "btnTimer";
            btnTimer.Size = new Size(168, 44);
            btnTimer.TabIndex = 4;
            btnTimer.Text = "timer";
            btnTimer.UseVisualStyleBackColor = true;
            // 
            // btnException
            // 
            btnException.Location = new Point(561, 212);
            btnException.Name = "btnException";
            btnException.Size = new Size(168, 44);
            btnException.TabIndex = 6;
            btnException.Text = "exception";
            btnException.UseVisualStyleBackColor = true;
            btnException.Click += btnException_Click;
            // 
            // btnContinueWith
            // 
            btnContinueWith.Location = new Point(561, 162);
            btnContinueWith.Name = "btnContinueWith";
            btnContinueWith.Size = new Size(168, 44);
            btnContinueWith.TabIndex = 5;
            btnContinueWith.Text = "continuewith";
            btnContinueWith.UseVisualStyleBackColor = true;
            btnContinueWith.Click += btnContinueWith_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnException);
            Controls.Add(btnContinueWith);
            Controls.Add(btnTimer);
            Controls.Add(btnAsyncVoid);
            Controls.Add(btnWait);
            Controls.Add(btnAwait);
            Controls.Add(listBox1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBox1;
        private Button btnAwait;
        private Button btnWait;
        private Button btnAsyncVoid;
        private Button btnTimer;
        private Button btnException;
        private Button btnContinueWith;
    }
}
