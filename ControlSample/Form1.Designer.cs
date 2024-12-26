namespace ControlSample
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
            dataGridView1 = new DataGridView();
            ColumnId = new DataGridViewTextBoxColumn();
            ColumnName = new DataGridViewTextBoxColumn();
            ColumnDesc = new DataGridViewTextBoxColumn();
            ucdd1 = new UcDD();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { ColumnId, ColumnName, ColumnDesc });
            dataGridView1.Location = new Point(12, 212);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(776, 226);
            dataGridView1.TabIndex = 0;
            dataGridView1.MouseDown += dataGridView1_MouseDown;
            // 
            // ColumnId
            // 
            ColumnId.DataPropertyName = "Id";
            ColumnId.HeaderText = "id";
            ColumnId.Name = "ColumnId";
            // 
            // ColumnName
            // 
            ColumnName.DataPropertyName = "Name";
            ColumnName.HeaderText = "Name";
            ColumnName.Name = "ColumnName";
            // 
            // ColumnDesc
            // 
            ColumnDesc.DataPropertyName = "Description ";
            ColumnDesc.HeaderText = "Desc";
            ColumnDesc.Name = "ColumnDesc";
            // 
            // ucdd1
            // 
            ucdd1.AllowDrop = true;
            ucdd1.BackColor = SystemColors.ActiveCaption;
            ucdd1.Location = new Point(12, 12);
            ucdd1.Name = "ucdd1";
            ucdd1.Size = new Size(435, 170);
            ucdd1.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(ucdd1);
            Controls.Add(dataGridView1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn ColumnId;
        private DataGridViewTextBoxColumn ColumnName;
        private DataGridViewTextBoxColumn ColumnDesc;
        private UcDD ucdd1;
    }
}
