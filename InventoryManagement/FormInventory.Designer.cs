namespace InventoryManagement
{
    partial class FormInventory
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
            textBox1 = new TextBox();
            dataGridView1 = new DataGridView();
            button2 = new Button();
            button3 = new Button();
            label2 = new Label();
            panel1 = new Panel();
            button1 = new Button();
            label1 = new Label();
            Column1 = new DataGridViewButtonColumn();
            Column8 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column6 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column7 = new DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            textBox1.Location = new Point(92, 118);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(303, 31);
            textBox1.TabIndex = 2;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Column1, Column8, Column2, Column6, Column3, Column5, Column4, Column7 });
            dataGridView1.Location = new Point(12, 152);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(1117, 582);
            dataGridView1.TabIndex = 3;
            // 
            // button2
            // 
            button2.BackColor = Color.SteelBlue;
            button2.FlatAppearance.BorderColor = Color.SteelBlue;
            button2.FlatAppearance.BorderSize = 2;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("メイリオ", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 128);
            button2.ForeColor = Color.White;
            button2.Location = new Point(12, 51);
            button2.Name = "button2";
            button2.Size = new Size(165, 47);
            button2.TabIndex = 4;
            button2.Text = "＋ 入庫";
            button2.UseVisualStyleBackColor = false;
            // 
            // button3
            // 
            button3.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            button3.Location = new Point(1037, 118);
            button3.Name = "button3";
            button3.Size = new Size(92, 33);
            button3.TabIndex = 5;
            button3.Text = "クリア";
            button3.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label2.Location = new Point(12, 121);
            label2.Name = "label2";
            label2.Size = new Size(74, 24);
            label2.TabIndex = 6;
            label2.Text = "フィルタ";
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(56, 56, 56);
            panel1.Controls.Add(button1);
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1141, 38);
            panel1.TabIndex = 7;
            // 
            // button1
            // 
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            button1.ForeColor = Color.White;
            button1.Location = new Point(1049, 0);
            button1.Name = "button1";
            button1.Size = new Size(92, 38);
            button1.TabIndex = 3;
            button1.Text = "戻る";
            button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("メイリオ", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 128);
            label1.ForeColor = Color.White;
            label1.Location = new Point(12, 5);
            label1.Name = "label1";
            label1.Size = new Size(211, 28);
            label1.TabIndex = 2;
            label1.Text = "在庫管理 > 在庫リスト";
            // 
            // Column1
            // 
            Column1.HeaderText = "詳細";
            Column1.Name = "Column1";
            // 
            // Column8
            // 
            Column8.HeaderText = "区分";
            Column8.Name = "Column8";
            // 
            // Column2
            // 
            Column2.HeaderText = "入荷番号";
            Column2.Name = "Column2";
            // 
            // Column6
            // 
            Column6.HeaderText = "S/N";
            Column6.Name = "Column6";
            // 
            // Column3
            // 
            Column3.HeaderText = "正式品番";
            Column3.Name = "Column3";
            Column3.Width = 300;
            // 
            // Column5
            // 
            Column5.HeaderText = "数量";
            Column5.Name = "Column5";
            // 
            // Column4
            // 
            Column4.HeaderText = "価格";
            Column4.Name = "Column4";
            // 
            // Column7
            // 
            Column7.HeaderText = "デモ機割り当て";
            Column7.Name = "Column7";
            // 
            // FormInventory
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(230, 234, 242);
            ClientSize = new Size(1141, 746);
            Controls.Add(panel1);
            Controls.Add(label2);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(dataGridView1);
            Controls.Add(textBox1);
            Name = "FormInventory";
            Text = "FormInventory";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox textBox1;
        private DataGridView dataGridView1;
        private Button button2;
        private Button button3;
        private Label label2;
        private Panel panel1;
        private Button button1;
        private Label label1;
        private DataGridViewButtonColumn Column1;
        private DataGridViewTextBoxColumn Column8;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column6;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column5;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewButtonColumn Column7;
    }
}