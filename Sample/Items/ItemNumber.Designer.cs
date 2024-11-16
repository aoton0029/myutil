namespace Sample.Items
{
    partial class ItemNumber
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
            btnUp = new Button();
            txtNumber = new TextBox();
            btnDown = new Button();
            SuspendLayout();
            // 
            // btnUp
            // 
            btnUp.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnUp.Location = new Point(144, 0);
            btnUp.Name = "btnUp";
            btnUp.Size = new Size(37, 36);
            btnUp.TabIndex = 33;
            btnUp.Text = "▲";
            btnUp.UseVisualStyleBackColor = true;
            btnUp.Click += btnUp_Click;
            // 
            // txtNumber
            // 
            txtNumber.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            txtNumber.Location = new Point(38, 2);
            txtNumber.Name = "txtNumber";
            txtNumber.Size = new Size(105, 31);
            txtNumber.TabIndex = 32;
            txtNumber.TextChanged += txtNumber_TextChanged;
            // 
            // btnDown
            // 
            btnDown.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnDown.Location = new Point(0, 0);
            btnDown.Name = "btnDown";
            btnDown.Size = new Size(37, 36);
            btnDown.TabIndex = 31;
            btnDown.Text = "▼";
            btnDown.UseVisualStyleBackColor = true;
            btnDown.Click += btnDown_Click;
            // 
            // ItemNumber
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnUp);
            Controls.Add(txtNumber);
            Controls.Add(btnDown);
            Name = "ItemNumber";
            Size = new Size(181, 36);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnUp;
        private TextBox txtNumber;
        private Button btnDown;
    }
}
