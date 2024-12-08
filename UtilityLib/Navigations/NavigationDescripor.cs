using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityLib.DIs;

namespace UtilityLib.Navigations
{
    public enum NavigationLifetime
    {
        Singleton,
        Transient
    }

    public class NavigationCollection
    {
        private readonly Dictionary<string, NavigationDescriptor> _descriptors = new();

        public void Register<T>(string key, NavigationLifetime lifetime) where T : UserControl
        {
            _descriptors[key] = new NavigationDescriptor(typeof(T), lifetime);
        }

        public NavigationDescriptor GetDescriptor(string key)
        {
            if (!_descriptors.TryGetValue(key, out var descriptor))
            {
                throw new Exception($"Navigation key '{key}' is not registered.");
            }
            return descriptor;
        }
    }

    public class NavigationDescriptor
    {
        public Type ControlType { get; }
        public NavigationLifetime Lifetime { get; }
        public UserControl SingletonInstance { get; private set; }

        public NavigationDescriptor(Type controlType, NavigationLifetime lifetime)
        {
            ControlType = controlType;
            Lifetime = lifetime;
        }

        public void SetInstance(UserControl instance)
        {
            SingletonInstance = instance;
        }
    }

    public interface IUserControl
    {
        void OnNavigated(params object[] args);
    }
    
    public interface INavigationService
    {
        void Initialize<T>() where T : UserControl;
        void SetMainPanel(Panel panel);
        void Navigate<T>(object parameter = null) where T : UserControl;
    }

    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private Control _parentControl;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Initialize<T>() where T : UserControl
        {
            Navigate<T>();
        }

        public void SetMainPanel(Panel panel)
        {
            _parentControl = panel ?? throw new ArgumentNullException(nameof(panel));
        }

        public void Navigate<T>(object parameter = null) where T : UserControl
        {
            if (_parentControl == null)
            {
                throw new InvalidOperationException("Main panel is not set.");
            }

            // 現在のUserControlを破棄
            if (_parentControl.Controls.Count > 0)
            {
                _parentControl.Controls[0].Dispose();
            }

            // 新しいUserControlを生成
            var control = _serviceProvider.GetService<T>();
            if (control is IUserControl navigable)
            {
                navigable.OnNavigatedTo(parameter);
            }

            // UserControlをPanelに追加
            _parentControl.Controls.Clear();
            _parentControl.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }
    }

    public class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public void Main()
        {
            var services = new ServiceCollection();

            // NavigationServiceを登録
            services.Append(ServiceDescriptor.Singleton<INavigationService, NavigationService>());

            // UserControlを登録
            services.Append(ServiceDescriptor.Singleton<IUserControl, UcHome>());
            services.Append(ServiceDescriptor.Singleton<IUserControl, UcSetting>());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ServiceProvider = new ServiceProvider(services);
            // MainFormを起動し、NavigationServiceで初期画面を設定
            INavigationService navigationService = ServiceProvider.GetService(typeof(INavigationService));
            navigationService.Initialize<UcHome>();

            Application.Run(new MainForm(navigationService));
        }
    }


}
