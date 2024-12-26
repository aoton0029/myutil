namespace ControlSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            List<MyDataObject> list = new List<MyDataObject>();
            list.Add(new MyDataObject() { Id =1, Name="SAMPLE1", Description="SAMPLE"});
            list.Add(new MyDataObject() { Id = 2, Name = "SAMPLE2", Description = "SAMPLE2" });

            dataGridView1.DataSource = list;
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {

            // ドラッグする行のデータを取得
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView1.SelectedRows[0];
                // 紐づいたデータソース（オブジェクト）を取得
                var data = selectedRow.DataBoundItem;
                if (data != null)
                {
                    // ドラッグを開始
                    dataGridView1.DoDragDrop(data, DragDropEffects.Move);
                }
            }
        }
    }
}
