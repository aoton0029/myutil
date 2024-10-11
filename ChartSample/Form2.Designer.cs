namespace ChartSample
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.grid = new System.Windows.Forms.DataGridView();
            this.ColumnWaveType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColumnStartFrequency = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnStopFrequency = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnIsSweepFrequency = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnStartAmplitude = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnStopAmplitude = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnIsSweepAmplitude = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnSymmetry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnStartDcOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnStopDcOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnIsSweepDcOffset = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnStepTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cartesianChart1 = new LiveCharts.WinForms.CartesianChart();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ファイルFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.grid.AllowUserToResizeRows = false;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnWaveType,
            this.ColumnStartFrequency,
            this.ColumnStopFrequency,
            this.ColumnIsSweepFrequency,
            this.ColumnStartAmplitude,
            this.ColumnStopAmplitude,
            this.ColumnIsSweepAmplitude,
            this.ColumnSymmetry,
            this.ColumnStartDcOffset,
            this.ColumnStopDcOffset,
            this.ColumnIsSweepDcOffset,
            this.ColumnStepTime});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grid.DefaultCellStyle = dataGridViewCellStyle4;
            this.grid.Location = new System.Drawing.Point(220, 617);
            this.grid.Name = "grid";
            this.grid.RowHeadersWidth = 25;
            this.grid.RowTemplate.Height = 27;
            this.grid.Size = new System.Drawing.Size(1102, 232);
            this.grid.TabIndex = 0;
            // 
            // ColumnWaveType
            // 
            this.ColumnWaveType.DataPropertyName = "WaveType";
            this.ColumnWaveType.HeaderText = "WaveType";
            this.ColumnWaveType.Name = "ColumnWaveType";
            this.ColumnWaveType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ColumnWaveType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.ColumnWaveType.Width = 150;
            // 
            // ColumnStartFrequency
            // 
            this.ColumnStartFrequency.DataPropertyName = "StartFrequency";
            this.ColumnStartFrequency.HeaderText = "StartFrequency";
            this.ColumnStartFrequency.Name = "ColumnStartFrequency";
            this.ColumnStartFrequency.Width = 80;
            // 
            // ColumnStopFrequency
            // 
            this.ColumnStopFrequency.DataPropertyName = "StopFrequency";
            this.ColumnStopFrequency.HeaderText = "StopFrequency";
            this.ColumnStopFrequency.Name = "ColumnStopFrequency";
            this.ColumnStopFrequency.Width = 80;
            // 
            // ColumnIsSweepFrequency
            // 
            this.ColumnIsSweepFrequency.DataPropertyName = "IsSweepFrequency";
            this.ColumnIsSweepFrequency.HeaderText = "IsSweepFrequency";
            this.ColumnIsSweepFrequency.Name = "ColumnIsSweepFrequency";
            this.ColumnIsSweepFrequency.Width = 80;
            // 
            // ColumnStartAmplitude
            // 
            this.ColumnStartAmplitude.DataPropertyName = "StartAmplitude";
            this.ColumnStartAmplitude.HeaderText = "StartAmplitude";
            this.ColumnStartAmplitude.Name = "ColumnStartAmplitude";
            this.ColumnStartAmplitude.Width = 80;
            // 
            // ColumnStopAmplitude
            // 
            this.ColumnStopAmplitude.DataPropertyName = "StopAmplitude";
            this.ColumnStopAmplitude.HeaderText = "StopAmplitude";
            this.ColumnStopAmplitude.Name = "ColumnStopAmplitude";
            this.ColumnStopAmplitude.Width = 80;
            // 
            // ColumnIsSweepAmplitude
            // 
            this.ColumnIsSweepAmplitude.DataPropertyName = "IsSweepAmplitude";
            this.ColumnIsSweepAmplitude.HeaderText = "IsSweepAmplitude";
            this.ColumnIsSweepAmplitude.Name = "ColumnIsSweepAmplitude";
            this.ColumnIsSweepAmplitude.Width = 80;
            // 
            // ColumnSymmetry
            // 
            this.ColumnSymmetry.DataPropertyName = "Symmetry";
            this.ColumnSymmetry.HeaderText = "Symmetry";
            this.ColumnSymmetry.Name = "ColumnSymmetry";
            this.ColumnSymmetry.Width = 120;
            // 
            // ColumnStartDcOffset
            // 
            this.ColumnStartDcOffset.DataPropertyName = "StartDcOffset";
            this.ColumnStartDcOffset.HeaderText = "StartDcOffset";
            this.ColumnStartDcOffset.Name = "ColumnStartDcOffset";
            this.ColumnStartDcOffset.Width = 80;
            // 
            // ColumnStopDcOffset
            // 
            this.ColumnStopDcOffset.DataPropertyName = "StopDcOffset";
            this.ColumnStopDcOffset.HeaderText = "StopDcOffset";
            this.ColumnStopDcOffset.Name = "ColumnStopDcOffset";
            this.ColumnStopDcOffset.Width = 80;
            // 
            // ColumnIsSweepDcOffset
            // 
            this.ColumnIsSweepDcOffset.DataPropertyName = "IsSweepDcOffset";
            this.ColumnIsSweepDcOffset.HeaderText = "IsSweepDcOffset";
            this.ColumnIsSweepDcOffset.Name = "ColumnIsSweepDcOffset";
            this.ColumnIsSweepDcOffset.Width = 80;
            // 
            // ColumnStepTime
            // 
            this.ColumnStepTime.DataPropertyName = "StepTime";
            this.ColumnStepTime.HeaderText = "StepTime(s)";
            this.ColumnStepTime.Name = "ColumnStepTime";
            this.ColumnStepTime.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ColumnStepTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColumnStepTime.Width = 120;
            // 
            // cartesianChart1
            // 
            this.cartesianChart1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(62)))), ((int)(((byte)(88)))));
            this.cartesianChart1.Location = new System.Drawing.Point(220, 78);
            this.cartesianChart1.Name = "cartesianChart1";
            this.cartesianChart1.Size = new System.Drawing.Size(1102, 497);
            this.cartesianChart1.TabIndex = 1;
            this.cartesianChart1.Text = "cartesianChart1";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(108)))), ((int)(((byte)(201)))));
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(1058, 27);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(129, 45);
            this.button1.TabIndex = 2;
            this.button1.Text = "Clamp";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(108)))), ((int)(((byte)(201)))));
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Location = new System.Drawing.Point(1193, 27);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(129, 45);
            this.button2.TabIndex = 3;
            this.button2.Text = "DeClamp";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 36);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(195, 29);
            this.comboBox1.TabIndex = 4;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ファイルFToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1334, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ファイルFToolStripMenuItem
            // 
            this.ファイルFToolStripMenuItem.Name = "ファイルFToolStripMenuItem";
            this.ファイルFToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.ファイルFToolStripMenuItem.Text = "ファイル(&F)";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1334, 861);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cartesianChart1);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form2";
            this.Text = "Form2";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView grid;
        private LiveCharts.WinForms.CartesianChart cartesianChart1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ファイルFToolStripMenuItem;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColumnWaveType;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnStartFrequency;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnStopFrequency;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnIsSweepFrequency;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnStartAmplitude;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnStopAmplitude;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnIsSweepAmplitude;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSymmetry;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnStartDcOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnStopDcOffset;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnIsSweepDcOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnStepTime;
    }
}