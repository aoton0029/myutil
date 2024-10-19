using PrintSample.Pages;

namespace PrintSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            NavigationService.Instance.Initialize(frame1);
            NavigationService.Instance.Navigate(new UcMain());
        }
    }
}
