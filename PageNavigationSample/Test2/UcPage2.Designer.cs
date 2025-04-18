﻿namespace PageNavigationSample.Test2
{
    partial class UcPage2
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
            btnNext = new Button();
            btnCancel = new Button();
            btnPrev = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // btnNext
            // 
            btnNext.Font = new Font("Yu Gothic UI", 24F);
            btnNext.Location = new Point(295, 98);
            btnNext.Margin = new Padding(3, 2, 3, 2);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(131, 57);
            btnNext.TabIndex = 7;
            btnNext.Text = "次";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Yu Gothic UI", 24F);
            btnCancel.Location = new Point(158, 98);
            btnCancel.Margin = new Padding(3, 2, 3, 2);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(131, 57);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "キャンセル";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnPrev
            // 
            btnPrev.Font = new Font("Yu Gothic UI", 24F);
            btnPrev.Location = new Point(22, 98);
            btnPrev.Margin = new Padding(3, 2, 3, 2);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(131, 57);
            btnPrev.TabIndex = 5;
            btnPrev.Text = "前";
            btnPrev.UseVisualStyleBackColor = true;
            btnPrev.Click += btnPrev_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 24F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label1.Location = new Point(22, 13);
            label1.Name = "label1";
            label1.Size = new Size(37, 45);
            label1.TabIndex = 4;
            label1.Text = "2";
            // 
            // UcPage2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(255, 255, 192);
            Controls.Add(btnNext);
            Controls.Add(btnCancel);
            Controls.Add(btnPrev);
            Controls.Add(label1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "UcPage2";
            Size = new Size(629, 379);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnNext;
        private Button btnCancel;
        private Button btnPrev;
        private Label label1;
    }
}
