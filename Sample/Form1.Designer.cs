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
            grid2 = new DataGridView();
            gridResult = new DataGridView();
            grid1 = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)grid2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridResult).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grid1).BeginInit();
            SuspendLayout();
            // 
            // grid2
            // 
            grid2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid2.Location = new Point(347, 12);
            grid2.Name = "grid2";
            grid2.Size = new Size(329, 453);
            grid2.TabIndex = 0;
            grid2.CellContentClick += dataGridView_CellContentClick;
            // 
            // gridResult
            // 
            gridResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridResult.Location = new Point(682, 12);
            gridResult.Name = "gridResult";
            gridResult.Size = new Size(329, 453);
            gridResult.TabIndex = 0;
            gridResult.CellContentClick += dataGridView1_CellContentClick;
            // 
            // grid1
            // 
            grid1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid1.Location = new Point(12, 12);
            grid1.Name = "grid1";
            grid1.Size = new Size(329, 453);
            grid1.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1017, 493);
            Controls.Add(grid1);
            Controls.Add(gridResult);
            Controls.Add(grid2);
            Name = "Form1";
            Text = "Form1";
            Shown += Form1_Shown;
            ((System.ComponentModel.ISupportInitialize)grid2).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridResult).EndInit();
            ((System.ComponentModel.ISupportInitialize)grid1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView grid2;
        private DataGridView gridResult;
        private DataGridView grid1;
    }
}
