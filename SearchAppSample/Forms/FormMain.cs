namespace SearchAppSample
{
    public partial class FormMain : Form
    {
        ServiceProvider _serviceProvider;

        public FormMain()
        {
            InitializeComponent();
            _serviceProvider = new ServiceProvider();
            _serviceProvider.RegisterSingleton(new EventBus());
            _serviceProvider.RegisterSingleton(new Core.AppContext());
            _serviceProvider.RegisterSingleton(new NavigationService(this, _serviceProvider));
        }
    }
}
