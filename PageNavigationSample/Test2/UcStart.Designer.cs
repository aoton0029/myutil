namespace PageNavigationSample.Test2
{
    partial class UcStart
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
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
            btnNext.Location = new Point(292, 102);
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
            btnCancel.Location = new Point(156, 102);
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
            btnPrev.Location = new Point(19, 102);
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
            label1.Location = new Point(19, 16);
            label1.Name = "label1";
            label1.Size = new Size(86, 45);
            label1.TabIndex = 4;
            label1.Text = "Start";
            // 
            // UcStart
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(255, 192, 255);
            Controls.Add(btnNext);
            Controls.Add(btnCancel);
            Controls.Add(btnPrev);
            Controls.Add(label1);
            Name = "UcStart";
            Size = new Size(605, 391);
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
