namespace PageNavigationSample.Test2
{
    partial class UcPage1
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            btnPrev = new Button();
            btnCancel = new Button();
            btnNext = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 24F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label1.Location = new Point(38, 27);
            label1.Name = "label1";
            label1.Size = new Size(45, 54);
            label1.TabIndex = 0;
            label1.Text = "1";
            // 
            // btnPrev
            // 
            btnPrev.Font = new Font("Yu Gothic UI", 24F);
            btnPrev.Location = new Point(38, 141);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(150, 76);
            btnPrev.TabIndex = 1;
            btnPrev.Text = "前";
            btnPrev.UseVisualStyleBackColor = true;
            btnPrev.Click += btnPrev_Click;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Yu Gothic UI", 24F);
            btnCancel.Location = new Point(194, 141);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(150, 76);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "キャンセル";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnNext
            // 
            btnNext.Font = new Font("Yu Gothic UI", 24F);
            btnNext.Location = new Point(350, 141);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(150, 76);
            btnNext.TabIndex = 3;
            btnNext.Text = "次";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // UcPage1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(255, 224, 192);
            Controls.Add(btnNext);
            Controls.Add(btnCancel);
            Controls.Add(btnPrev);
            Controls.Add(label1);
            Name = "UcPage1";
            Size = new Size(675, 459);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button btnPrev;
        private Button btnCancel;
        private Button btnNext;
    }
}
