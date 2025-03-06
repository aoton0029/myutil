namespace PageNavigationSample
{
    public partial class Form1 : Form
    {
        private readonly DataMediator _dataMediator = new();
        NavigationService _nav;
        SharedData _data;

        public Form1()
        {
            InitializeComponent();

            _dataMediator = new DataMediator();
            _data = new SharedData() { UserName = "TEST", Id = 9 };
            _dataMediator.Register("Number", _data);
            _nav = new NavigationService(this, null);
            _nav.Register(() => new UcPage1(_nav, _dataMediator));
            _nav.Register(() => new UcPage2(_nav, _dataMediator));
            _nav.Register(() => new UcPage3(_nav, _dataMediator));

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            _nav.Navigate<UcPage1>();

        }
    }
}
