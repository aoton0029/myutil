ServiceProvider は、SharedData を ServiceCollection に シングルトン として登録し、画面 (UserControl) に渡しています。


---

どこで SharedData を渡しているのか？

SharedData の登録は MainForm のコンストラクタ内 で行っています。

MainForm.cs

public class MainForm : Form
{
    private readonly ServiceProvider _serviceProvider;
    private readonly NavigationService _navigationService;
    private Panel _panel;

    public MainForm()
    {
        Width = 400;
        Height = 250;

        // ① ServiceCollection に SharedData をシングルトンとして登録
        var services = new ServiceCollection();
        services.AddSingleton(new SharedData()); // ← ここで登録！
        _serviceProvider = services.BuildServiceProvider();

        _panel = new Panel { Dock = DockStyle.Fill };
        Controls.Add(_panel);

        // ② NavigationService を作成し、DI コンテナを渡す
        _navigationService = new NavigationService(_panel, _serviceProvider);

        // ③ 画面を登録
        _navigationService.RegisterPage<Page1>();
        _navigationService.RegisterPage<Page2>();
        _navigationService.RegisterPage<Page3>();
        _navigationService.RegisterPage<Page4>();

        // ④ 最初のページを表示
        _navigationService.NavigateTo<Page1>();
    }
}

SharedData を ServiceCollection に シングルトンとして登録

ServiceProvider を構築 (BuildServiceProvider)

NavigationService に渡す

NavigationService は UserControl を管理



---

UserControl の中で SharedData をどう取得するのか？

各 UserControl (Page1, Page2, Page3, Page4) では、コンストラクタで SharedData を受け取る設計 になっています。

例えば Page1.cs では:

public class Page1 : UserControl
{
    private readonly SharedData _sharedData;

    public Page1(NavigationService navigation, SharedData sharedData)
    {
        _sharedData = sharedData; // ← ここで受け取る
    }
}

この Page1 のインスタンスを作るとき、NavigationService は ServiceProvider から SharedData を取得します。


---

どこで SharedData を Page1 に渡しているのか？

NavigationService の RegisterPage<TPage>() メソッドで UserControl を作成していますが、
UserControl は SharedData を必要とするので、ServiceProvider から取得するように修正します。

NavigationService.cs（修正後）

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;

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

    public void RegisterPage<TPage>() where TPage : UserControl
    {
        var pageType = typeof(TPage);
        
        // コンストラクタの引数を解決する
        var constructor = pageType.GetConstructors().First();
        var parameters = constructor.GetParameters()
            .Select(p => _serviceProvider.GetService(p.ParameterType))
            .ToArray();
        
        var pageInstance = (TPage)Activator.CreateInstance(pageType, parameters);
        _pages[pageType] = pageInstance;
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

変更点

1. RegisterPage<TPage>() のときに、ページのコンストラクタの引数を ServiceProvider から取得


2. SharedData を 自動で注入 できるようになった




---

最終的な流れ

MainForm

1. ServiceCollection に SharedData を登録


2. ServiceProvider を作成


3. NavigationService に ServiceProvider を渡す



NavigationService

4. RegisterPage<TPage>() で ServiceProvider から SharedData を取得


5. UserControl に SharedData を渡してインスタンスを作成



各 UserControl

6. SharedData にアクセスしてデータを保存・取得




---

結論

SharedData は MainForm で ServiceCollection に登録

NavigationService で ServiceProvider から SharedData を取得

UserControl では SharedData をコンストラクタ経由で受け取る


こうすることで、各ページ (UserControl) 間で データを共有 しながら、画面遷移がスムーズに できるようになっています！




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





この機構を使って 複数画面の入力フォーム を作成します。
「前へ」「次へ」ボタンで画面遷移し、最後に入力データを表示する」 という流れを実装します。


---

構成

1. SharedData : 入力データを保持


2. Page1 : 名前の入力


3. Page2 : 年齢の入力


4. Page3 : 住所の入力


5. Page4 : 入力確認画面




---

1. 共有データの管理

入力データを保持する SharedData を定義します。

SharedData.cs

public class SharedData
{
    public string Name { get; set; } = "";
    public int Age { get; set; } = 0;
    public string Address { get; set; } = "";
}


---

2. 画面の実装

各 UserControl に入力フォームを設置し、前後の画面に遷移できるようにします。


---

Page1.cs (名前の入力)

using System;
using System.Windows.Forms;

public class Page1 : UserControl
{
    private readonly NavigationService _navigation;
    private readonly SharedData _sharedData;
    private TextBox _txtName;
    private Button _btnNext;

    public Page1(NavigationService navigation, SharedData sharedData)
    {
        _navigation = navigation;
        _sharedData = sharedData;

        Label lbl = new Label { Text = "名前:", Left = 10, Top = 10 };
        _txtName = new TextBox { Left = 60, Top = 10, Width = 200, Text = _sharedData.Name };
        _btnNext = new Button { Left = 10, Top = 50, Text = "次へ" };

        _btnNext.Click += (s, e) =>
        {
            _sharedData.Name = _txtName.Text;
            _navigation.NavigateTo<Page2>();
        };

        Controls.Add(lbl);
        Controls.Add(_txtName);
        Controls.Add(_btnNext);
    }
}


---

Page2.cs (年齢の入力)

using System;
using System.Windows.Forms;

public class Page2 : UserControl
{
    private readonly NavigationService _navigation;
    private readonly SharedData _sharedData;
    private NumericUpDown _numAge;
    private Button _btnNext, _btnBack;

    public Page2(NavigationService navigation, SharedData sharedData)
    {
        _navigation = navigation;
        _sharedData = sharedData;

        Label lbl = new Label { Text = "年齢:", Left = 10, Top = 10 };
        _numAge = new NumericUpDown { Left = 60, Top = 10, Width = 100, Value = _sharedData.Age };
        _btnBack = new Button { Left = 10, Top = 50, Text = "戻る" };
        _btnNext = new Button { Left = 100, Top = 50, Text = "次へ" };

        _btnBack.Click += (s, e) => _navigation.NavigateTo<Page1>();
        _btnNext.Click += (s, e) =>
        {
            _sharedData.Age = (int)_numAge.Value;
            _navigation.NavigateTo<Page3>();
        };

        Controls.Add(lbl);
        Controls.Add(_numAge);
        Controls.Add(_btnBack);
        Controls.Add(_btnNext);
    }
}


---

Page3.cs (住所の入力)

using System;
using System.Windows.Forms;

public class Page3 : UserControl
{
    private readonly NavigationService _navigation;
    private readonly SharedData _sharedData;
    private TextBox _txtAddress;
    private Button _btnNext, _btnBack;

    public Page3(NavigationService navigation, SharedData sharedData)
    {
        _navigation = navigation;
        _sharedData = sharedData;

        Label lbl = new Label { Text = "住所:", Left = 10, Top = 10 };
        _txtAddress = new TextBox { Left = 60, Top = 10, Width = 200, Text = _sharedData.Address };
        _btnBack = new Button { Left = 10, Top = 50, Text = "戻る" };
        _btnNext = new Button { Left = 100, Top = 50, Text = "次へ" };

        _btnBack.Click += (s, e) => _navigation.NavigateTo<Page2>();
        _btnNext.Click += (s, e) =>
        {
            _sharedData.Address = _txtAddress.Text;
            _navigation.NavigateTo<Page4>();
        };

        Controls.Add(lbl);
        Controls.Add(_txtAddress);
        Controls.Add(_btnBack);
        Controls.Add(_btnNext);
    }
}


---

Page4.cs (入力確認画面)

using System;
using System.Windows.Forms;

public class Page4 : UserControl
{
    private readonly NavigationService _navigation;
    private readonly SharedData _sharedData;
    private Label _lblSummary;
    private Button _btnBack, _btnSubmit;

    public Page4(NavigationService navigation, SharedData sharedData)
    {
        _navigation = navigation;
        _sharedData = sharedData;

        _lblSummary = new Label
        {
            Left = 10, 
            Top = 10, 
            Width = 300,
            Height = 80,
            Text = $"名前: {_sharedData.Name}\n年齢: {_sharedData.Age}\n住所: {_sharedData.Address}"
        };

        _btnBack = new Button { Left = 10, Top = 100, Text = "戻る" };
        _btnSubmit = new Button { Left = 100, Top = 100, Text = "送信" };

        _btnBack.Click += (s, e) => _navigation.NavigateTo<Page3>();
        _btnSubmit.Click += (s, e) =>
        {
            MessageBox.Show("データを送信しました！", "送信完了");
            _navigation.NavigateTo<Page1>();
        };

        Controls.Add(_lblSummary);
        Controls.Add(_btnBack);
        Controls.Add(_btnSubmit);
    }
}


---

3. メインフォームのセットアップ

MainForm.cs に NavigationService を設定し、画面遷移を管理します。

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
        Height = 250;

        var services = new ServiceCollection();
        services.AddSingleton(new SharedData());
        _serviceProvider = services.BuildServiceProvider();

        _panel = new Panel { Dock = DockStyle.Fill };
        Controls.Add(_panel);

        _navigationService = new NavigationService(_panel, _serviceProvider);
        _navigationService.RegisterPage<Page1>();
        _navigationService.RegisterPage<Page2>();
        _navigationService.RegisterPage<Page3>();
        _navigationService.RegisterPage<Page4>();

        _navigationService.NavigateTo<Page1>();
    }
}


---

4. エントリーポイント

最後に Program.cs でアプリを起動。

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

1. Page1 で名前を入力 → 次へ


2. Page2 で年齢を入力 → 次へ / 戻る


3. Page3 で住所を入力 → 次へ / 戻る


4. Page4 で入力内容を確認 → 送信 / 戻る


5. 送信すると Page1 に戻る




---

ポイント

SharedData に値を格納して画面間で共有

NavigationService を使って UserControl を切り替え

ユーザーが戻ったときも入力データを保持


この仕組みを応用すれば、さまざまな ウィザード形式のフォーム を作れます！


