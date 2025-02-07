WinFormsでシンプルなDIコンテナ（ServiceCollection、ServiceProvider、ServiceDescriptor）とNavigationServiceを独自実装し、UserControlを使った画面遷移を行う仕組みを作ります。また、画面間で共有データを保持する仕組みも組み込みます。


---

1. 簡易 DI コンテナを実装

ServiceCollection はサービスの登録を行い、ServiceProvider がそれらを解決します。

ServiceCollection.cs

using System;
using System.Collections.Generic;
using System.Linq;

public class ServiceCollection
{
    private readonly List<ServiceDescriptor> _services = new();

    public void AddSingleton<TService, TImplementation>() where TImplementation : TService, new()
    {
        _services.Add(new ServiceDescriptor(typeof(TService), new TImplementation()));
    }

    public void AddSingleton<TService>(TService instance)
    {
        _services.Add(new ServiceDescriptor(typeof(TService), instance));
    }

    public ServiceProvider BuildServiceProvider()
    {
        return new ServiceProvider(_services);
    }
}

public class ServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();

    public ServiceProvider(IEnumerable<ServiceDescriptor> services)
    {
        foreach (var descriptor in services)
        {
            _services[descriptor.ServiceType] = descriptor.Implementation;
        }
    }

    public T GetService<T>()
    {
        return (T)_services[typeof(T)];
    }
}

public class ServiceDescriptor
{
    public Type ServiceType { get; }
    public object Implementation { get; }

    public ServiceDescriptor(Type serviceType, object implementation)
    {
        ServiceType = serviceType;
        Implementation = implementation;
    }
}


---

2. NavigationService の実装

NavigationService を使って UserControl の切り替えを行います。

NavigationService.cs

using System;
using System.Collections.Generic;
using System.Windows.Forms;

public class NavigationService
{
    private readonly Panel _hostPanel;
    private readonly Dictionary<Type, UserControl> _pages = new();
    private readonly ServiceProvider _serviceProvider;

    public NavigationService(Panel hostPanel, ServiceProvider serviceProvider)
    {
        _hostPanel = hostPanel;
        _serviceProvider = serviceProvider;
    }

    public void RegisterPage<TPage>() where TPage : UserControl, new()
    {
        var page = new TPage();
        _pages[typeof(TPage)] = page;
    }

    public void NavigateTo<TPage>() where TPage : UserControl
    {
        if (_pages.TryGetValue(typeof(TPage), out var page))
        {
            _hostPanel.Controls.Clear();
            _hostPanel.Controls.Add(page);
            page.Dock = DockStyle.Fill;
        }
        else
        {
            throw new InvalidOperationException($"Page {typeof(TPage).Name} is not registered.");
        }
    }
}


---

3. 共有データの管理

画面間で共有するデータを SharedData クラスとして管理し、DIコンテナに登録します。

SharedData.cs

public class SharedData
{
    public string SomeValue { get; set; } = "初期値";
}


---

4. ユーザーコントロールの作成

2つの画面 Page1 と Page2 を作成し、データの受け渡しを行います。

Page1.cs

using System;
using System.Windows.Forms;

public class Page1 : UserControl
{
    private readonly NavigationService _navigation;
    private readonly SharedData _sharedData;
    private TextBox _textBox;
    private Button _button;

    public Page1(NavigationService navigation, SharedData sharedData)
    {
        _navigation = navigation;
        _sharedData = sharedData;

        _textBox = new TextBox { Left = 10, Top = 10, Width = 200, Text = _sharedData.SomeValue };
        _button = new Button { Left = 10, Top = 40, Text = "Page2へ" };

        _button.Click += (s, e) =>
        {
            _sharedData.SomeValue = _textBox.Text;
            _navigation.NavigateTo<Page2>();
        };

        Controls.Add(_textBox);
        Controls.Add(_button);
    }
}


---

Page2.cs

using System;
using System.Windows.Forms;

public class Page2 : UserControl
{
    private readonly NavigationService _navigation;
    private readonly SharedData _sharedData;
    private Label _label;
    private Button _buttonBack;

    public Page2(NavigationService navigation, SharedData sharedData)
    {
        _navigation = navigation;
        _sharedData = sharedData;

        _label = new Label { Left = 10, Top = 10, Width = 200, Text = "値: " + _sharedData.SomeValue };
        _buttonBack = new Button { Left = 10, Top = 40, Text = "戻る" };

        _buttonBack.Click += (s, e) => _navigation.NavigateTo<Page1>();

        Controls.Add(_label);
        Controls.Add(_buttonBack);
    }
}


---

5. メインフォームの実装

WinForms の Form を作成し、DIコンテナとナビゲーションサービスを組み込みます。

MainForm.cs

using System;
using System.Windows.Forms;

public class MainForm : Form
{
    private readonly ServiceProvider _serviceProvider;
    private readonly NavigationService _navigationService;
    private Panel _panel;

    public MainForm()
    {
        Width = 400;
        Height = 300;

        var services = new ServiceCollection();
        services.AddSingleton(new SharedData()); // 共有データ
        _serviceProvider = services.BuildServiceProvider();

        _panel = new Panel { Dock = DockStyle.Fill };
        Controls.Add(_panel);

        _navigationService = new NavigationService(_panel, _serviceProvider);
        _navigationService.RegisterPage<Page1>();
        _navigationService.RegisterPage<Page2>();

        // 最初のページに遷移
        _navigationService.NavigateTo<Page1>();
    }
}


---

6. エントリーポイント

最後に、Program.cs でアプリを起動します。

Program.cs

using System;
using System.Windows.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}


---

動作の流れ

1. MainForm で NavigationService を作成し、UserControl を使った画面遷移を管理。


2. SharedData をDIコンテナで管理し、Page1 で入力した値を Page2 で表示。


3. Page1 の TextBox で入力 → Page2 に遷移すると、Label に値が反映。




---

この実装のポイント

独自 DI コンテナ

ServiceCollection にサービスを登録。

ServiceProvider がインスタンスを管理し、共有データを提供。


NavigationService

UserControl を Panel に追加して動的に画面を切り替え。


共有データ

SharedData を ServiceCollection に登録し、UserControl 間で共有。




---

これで、WinForms で UserControl を使ったシンプルな DI と画面遷移の仕組みを実現できます。

