namespace Sample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void changeUc(UserControl uc)
        {
            pnlMain.Controls.Clear();
            pnlMain.Controls.Add(uc);
        }

        private void rdbSerail_CheckedChanged(object sender, EventArgs e)
        {
            changeUc(new UcSerial());
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            changeUc(new UcSerial());
        }
    }
}
