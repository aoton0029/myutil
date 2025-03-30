namespace AsyncSample
{
    partial class Form2
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
            buttonFixedInterval = new Button();
            button1 = new Button();
            button3 = new Button();
            buttonCatchUp = new Button();
            button5 = new Button();
            buttonSkip = new Button();
            button7 = new Button();
            buttonFail = new Button();
            buttonStopStoppable = new Button();
            buttonStoppable = new Button();
            SuspendLayout();
            // 
            // buttonFixedInterval
            // 
            buttonFixedInterval.Location = new Point(64, 34);
            buttonFixedInterval.Name = "buttonFixedInterval";
            buttonFixedInterval.Size = new Size(119, 47);
            buttonFixedInterval.TabIndex = 0;
            buttonFixedInterval.Text = "FixedInterval ";
            buttonFixedInterval.UseVisualStyleBackColor = true;
            buttonFixedInterval.Click += buttonFixedInterval_Click;
            // 
            // button1
            // 
            button1.Location = new Point(189, 34);
            button1.Name = "button1";
            button1.Size = new Size(119, 47);
            button1.TabIndex = 1;
            button1.Text = "STOP";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button3
            // 
            button3.Location = new Point(189, 87);
            button3.Name = "button3";
            button3.Size = new Size(119, 47);
            button3.TabIndex = 3;
            button3.Text = "STOP";
            button3.UseVisualStyleBackColor = true;
            // 
            // buttonCatchUp
            // 
            buttonCatchUp.Location = new Point(64, 87);
            buttonCatchUp.Name = "buttonCatchUp";
            buttonCatchUp.Size = new Size(119, 47);
            buttonCatchUp.TabIndex = 2;
            buttonCatchUp.Text = "CatchUp";
            buttonCatchUp.UseVisualStyleBackColor = true;
            buttonCatchUp.Click += buttonCatchUp_Click;
            // 
            // button5
            // 
            button5.Location = new Point(189, 140);
            button5.Name = "button5";
            button5.Size = new Size(119, 47);
            button5.TabIndex = 5;
            button5.Text = "STOP";
            button5.UseVisualStyleBackColor = true;
            // 
            // buttonSkip
            // 
            buttonSkip.Location = new Point(64, 140);
            buttonSkip.Name = "buttonSkip";
            buttonSkip.Size = new Size(119, 47);
            buttonSkip.TabIndex = 4;
            buttonSkip.Text = "Skip";
            buttonSkip.UseVisualStyleBackColor = true;
            buttonSkip.Click += buttonSkip_Click;
            // 
            // button7
            // 
            button7.Location = new Point(189, 193);
            button7.Name = "button7";
            button7.Size = new Size(119, 47);
            button7.TabIndex = 7;
            button7.Text = "STOP";
            button7.UseVisualStyleBackColor = true;
            // 
            // buttonFail
            // 
            buttonFail.Location = new Point(64, 193);
            buttonFail.Name = "buttonFail";
            buttonFail.Size = new Size(119, 47);
            buttonFail.TabIndex = 6;
            buttonFail.Text = "Fail";
            buttonFail.UseVisualStyleBackColor = true;
            buttonFail.Click += buttonFail_Click;
            // 
            // buttonStopStoppable
            // 
            buttonStopStoppable.Location = new Point(189, 246);
            buttonStopStoppable.Name = "buttonStopStoppable";
            buttonStopStoppable.Size = new Size(119, 47);
            buttonStopStoppable.TabIndex = 9;
            buttonStopStoppable.Text = "STOP Stoppable";
            buttonStopStoppable.UseVisualStyleBackColor = true;
            buttonStopStoppable.Click += buttonStopStoppable_Click;
            // 
            // buttonStoppable
            // 
            buttonStoppable.Location = new Point(64, 246);
            buttonStoppable.Name = "buttonStoppable";
            buttonStoppable.Size = new Size(119, 47);
            buttonStoppable.TabIndex = 8;
            buttonStoppable.Text = "Stoppable";
            buttonStoppable.UseVisualStyleBackColor = true;
            buttonStoppable.Click += buttonStoppable_Click;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(buttonStopStoppable);
            Controls.Add(buttonStoppable);
            Controls.Add(button7);
            Controls.Add(buttonFail);
            Controls.Add(button5);
            Controls.Add(buttonSkip);
            Controls.Add(button3);
            Controls.Add(buttonCatchUp);
            Controls.Add(button1);
            Controls.Add(buttonFixedInterval);
            Name = "Form2";
            Text = "Form2";
            Shown += Form2_Shown;
            ResumeLayout(false);
        }

        #endregion
        private Button buttonFixedInterval;
        private Button button1;
        private Button button3;
        private Button buttonCatchUp;
        private Button button5;
        private Button buttonSkip;
        private Button button7;
        private Button buttonFail;
        private Button buttonStopStoppable;
        private Button buttonStoppable;
    }
}