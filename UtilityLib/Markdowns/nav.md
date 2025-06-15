ご質問ありがとうございます。Winformsで事前に画面を登録し、フロー内容を決定しておく実装についてご説明します。

## 事前に画面を登録するフローアーキテクチャの実装

前回説明したアーキテクチャをベースに、事前に画面を登録してフロー内容を定義できる機能を追加します。

### 1. フロー定義クラス

```csharp
public class FlowDefinition
{
    public string FlowId { get; }
    public string FlowName { get; }
    private readonly List<Type> _pageTypes = new List<Type>();
    
    public FlowDefinition(string flowId, string flowName)
    {
        FlowId = flowId;
        FlowName = flowName;
    }
    
    // 画面の登録
    public FlowDefinition AddPage<T>() where T : UcFlowPageBase
    {
        _pageTypes.Add(typeof(T));
        return this;
    }
    
    // 登録されている画面タイプのリストを取得
    public IReadOnlyList<Type> GetPageTypes() => _pageTypes.AsReadOnly();
}
```

### 2. フロー管理クラス

```csharp
public class FlowRegistry
{
    private readonly Dictionary<string, FlowDefinition> _flows = new Dictionary<string, FlowDefinition>();
    
    // フローの登録
    public void RegisterFlow(FlowDefinition flowDefinition)
    {
        _flows[flowDefinition.FlowId] = flowDefinition;
    }
    
    // フロー定義の取得
    public FlowDefinition GetFlow(string flowId)
    {
        if (!_flows.ContainsKey(flowId))
            throw new ArgumentException($"指定されたフローID {flowId} は登録されていません。");
        
        return _flows[flowId];
    }
    
    // 登録済みのすべてのフロー定義を取得
    public IEnumerable<FlowDefinition> GetAllFlows() => _flows.Values;
}
```

### 3. 拡張版NavigationFlowService

```csharp
public class NavigationFlowService
{
    private readonly Panel _containerPanel;
    private readonly SnapshotManager _snapshotManager;
    private readonly Stack<UcFlowPageBase> _navigationStack = new Stack<UcFlowPageBase>();
    private readonly BreadCrumb _breadCrumb;
    
    private FlowDefinition _currentFlow;
    private int _currentPageIndex = -1;
    
    public NavigationFlowService(Panel containerPanel, BreadCrumb breadCrumb)
    {
        _containerPanel = containerPanel;
        _breadCrumb = breadCrumb;
        _snapshotManager = new SnapshotManager();
    }
    
    // フローの開始
    public void StartFlow(FlowDefinition flow)
    {
        _currentFlow = flow;
        _currentPageIndex = -1;
        _navigationStack.Clear();
        _snapshotManager.ClearSnapshots();
        
        NavigateToNextPage();
    }
    
    // 次の画面へ移動
    public void NavigateToNextPage()
    {
        // 現在の画面を保存
        if (_containerPanel.Controls.Count > 0)
        {
            var currentPage = _containerPanel.Controls[0] as UcFlowPageBase;
            if (currentPage != null && currentPage is ISnapshot snapshot)
            {
                if (!snapshot.Validate()) return; // 検証失敗時は遷移しない
                _snapshotManager.SaveSnapshot(currentPage.PageId, snapshot);
                _navigationStack.Push(currentPage);
            }
        }
        
        _currentPageIndex++;
        var pageTypes = _currentFlow.GetPageTypes();
        
        // フローの最後に到達したかどうかを確認
        if (_currentPageIndex >= pageTypes.Count)
        {
            CompletedFlow();
            return;
        }
        
        // 次のページを作成して表示
        var nextPageType = pageTypes[_currentPageIndex];
        var nextPage = Activator.CreateInstance(nextPageType) as UcFlowPageBase;
        
        if (nextPage == null)
            throw new InvalidOperationException($"ページを作成できません: {nextPageType.Name}");
            
        nextPage.SetNavigationService(this);
        
        _containerPanel.Controls.Clear();
        _containerPanel.Controls.Add(nextPage);
        nextPage.Dock = DockStyle.Fill;
        
        UpdateBreadCrumb();
    }
    
    // フロー完了時の処理
    private void CompletedFlow()
    {
        // フロー完了イベントの発火など
        FlowCompleted?.Invoke(this, EventArgs.Empty);
    }
    
    // フロー完了イベント
    public event EventHandler FlowCompleted;
    
    // 前の画面に戻る
    public void NavigateBack()
    {
        if (_navigationStack.Count > 0)
        {
            _currentPageIndex--;
            _containerPanel.Controls.Clear();
            var previousPage = _navigationStack.Pop();
            _containerPanel.Controls.Add(previousPage);
            
            if (previousPage is ISnapshot snapshot)
            {
                _snapshotManager.RestoreSnapshot(previousPage.PageId, snapshot);
            }
            
            UpdateBreadCrumb();
        }
    }
    
    // 任意のインデックスのページに移動
    public void NavigateToPageIndex(int index)
    {
        if (index < 0 || index >= _currentFlow.GetPageTypes().Count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        // 現在のインデックスよりも前に戻る場合
        while (_currentPageIndex > index && _navigationStack.Count > 0)
        {
            NavigateBack();
        }
        
        // 現在のインデックスよりも先に進む場合
        while (_currentPageIndex < index)
        {
            NavigateToNextPage();
        }
    }
    
    // パンくずリスト更新
    private void UpdateBreadCrumb()
    {
        var pageTypes = _currentFlow.GetPageTypes();
        var breadcrumbItems = new List<BreadcrumbItem>();
        
        for (int i = 0; i <= _currentPageIndex && i < pageTypes.Count; i++)
        {
            var pageType = pageTypes[i];
            var instance = Activator.CreateInstance(pageType) as UcFlowPageBase;
            breadcrumbItems.Add(new BreadcrumbItem
            {
                Title = instance.PageTitle,
                Index = i,
                IsActive = i == _currentPageIndex
            });
        }
        
        _breadCrumb.UpdateItems(breadcrumbItems);
    }
}

public class BreadcrumbItem
{
    public string Title { get; set; }
    public int Index { get; set; }
    public bool IsActive { get; set; }
}
```

### 4. パンくずリストの拡張

```csharp
public class BreadCrumb : Control
{
    private List<BreadcrumbItem> _items = new List<BreadcrumbItem>();
    private NavigationFlowService _navigationService;
    
    public void SetNavigationService(NavigationFlowService service)
    {
        _navigationService = service;
    }
    
    public void UpdateItems(List<BreadcrumbItem> items)
    {
        _items = items;
        Invalidate(); // 再描画
    }
    
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        // パンくずリストの描画ロジック
        float x = 10;
        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            
            using (var brush = item.IsActive ? Brushes.Blue : Brushes.Black)
            {
                e.Graphics.DrawString(item.Title, this.Font, brush, x, 5);
            }
            
            // 項目の幅を計算（簡略化）
            var size = TextRenderer.MeasureText(item.Title, this.Font);
            x += size.Width;
            
            // 矢印を描画（最後の項目以外）
            if (i < _items.Count - 1)
            {
                e.Graphics.DrawString(" > ", this.Font, Brushes.Gray, x, 5);
                x += TextRenderer.MeasureText(" > ", this.Font).Width;
            }
        }
    }
    
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        
        if (_navigationService == null) return;
        
        // クリック位置から対応するパンくず項目を特定
        float x = 10;
        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var size = TextRenderer.MeasureText(item.Title, this.Font);
            
            if (e.X >= x && e.X <= x + size.Width)
            {
                // すでに完了しているページへの移動のみ許可
                if (item.Index < _items.Count - 1)
                {
                    _navigationService.NavigateToPageIndex(item.Index);
                }
                break;
            }
            
            x += size.Width;
            if (i < _items.Count - 1)
            {
                x += TextRenderer.MeasureText(" > ", this.Font).Width;
            }
        }
    }
}
```

### 5. 使用例

#### フローの定義

```csharp
// アプリケーション起動時などでフローを定義
public void ConfigureFlows(FlowRegistry registry)
{
    // 顧客登録フロー
    var customerRegistrationFlow = new FlowDefinition("customer-registration", "顧客登録")
        .AddPage<CustomerBasicInfoPage>()
        .AddPage<CustomerAddressPage>()
        .AddPage<CustomerContactPage>()
        .AddPage<CustomerConfirmationPage>();
        
    registry.RegisterFlow(customerRegistrationFlow);
    
    // 商品注文フロー
    var orderFlow = new FlowDefinition("order", "商品注文")
        .AddPage<ProductSelectionPage>()
        .AddPage<OrderQuantityPage>()
        .AddPage<ShippingInfoPage>()
        .AddPage<PaymentInfoPage>()
        .AddPage<OrderConfirmationPage>();
        
    registry.RegisterFlow(orderFlow);
}
```

#### フローの実行

```csharp
public partial class MainForm : Form
{
    private readonly FlowRegistry _flowRegistry = new FlowRegistry();
    private NavigationFlowService _navigationService;
    
    public MainForm()
    {
        InitializeComponent();
        
        // パンくずリストの初期化
        var breadCrumb = new BreadCrumb();
        breadCrumb.Dock = DockStyle.Top;
        breadCrumb.Height = 40;
        this.Controls.Add(breadCrumb);
        
        // ナビゲーションサービスの初期化
        _navigationService = new NavigationFlowService(panelContainer, breadCrumb);
        breadCrumb.SetNavigationService(_navigationService);
        
        // フロー完了時の処理
        _navigationService.FlowCompleted += (s, e) => {
            MessageBox.Show("フローが完了しました！");
            // フロー完了後の処理（データ保存など）
        };
        
        // フローの設定
        ConfigureFlows(_flowRegistry);
        
        // UIの初期化
        InitializeFlowSelectionUI();
    }
    
    private void InitializeFlowSelectionUI()
    {
        // フロー選択用のコンボボックスを作成
        var cboFlows = new ComboBox();
        cboFlows.Dock = DockStyle.Top;
        cboFlows.DropDownStyle = ComboBoxStyle.DropDownList;
        
        // 登録されているフローをコンボボックスに追加
        foreach (var flow in _flowRegistry.GetAllFlows())
        {
            cboFlows.Items.Add(new { Text = flow.FlowName, Value = flow.FlowId });
        }
        
        cboFlows.DisplayMember = "Text";
        cboFlows.ValueMember = "Value";
        
        // フロー開始ボタン
        var btnStartFlow = new Button();
        btnStartFlow.Text = "フローを開始";
        btnStartFlow.Click += (s, e) => {
            if (cboFlows.SelectedItem != null)
            {
                var selectedFlowId = (cboFlows.SelectedItem as dynamic).Value.ToString();
                var flow = _flowRegistry.GetFlow(selectedFlowId);
                _navigationService.StartFlow(flow);
            }
        };
        
        // コントロールの配置
        var flowSelectionPanel = new Panel();
        flowSelectionPanel.Dock = DockStyle.Top;
        flowSelectionPanel.Height = 80;
        
        flowSelectionPanel.Controls.Add(btnStartFlow);
        flowSelectionPanel.Controls.Add(cboFlows);
        
        this.Controls.Add(flowSelectionPanel);
    }
}
```

#### 画面の実装例

```csharp
// 顧客基本情報入力画面
public class CustomerBasicInfoPage : UcFlowPageBase
{
    private TextBox txtName;
    private TextBox txtEmail;
    private DateTimePicker dtpBirthDate;
    private Button btnNext;
    private Button btnCancel;
    
    public CustomerBasicInfoPage() : base("customer-basic", "基本情報入力")
    {
        InitializeComponent();
        
        btnNext.Click += (s, e) => NavigationService.NavigateToNextPage();
        btnCancel.Click += (s, e) => {
            // キャンセル処理
            if (MessageBox.Show("登録をキャンセルしますか？", "確認", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // メイン画面に戻るなどの処理
            }
        };
    }
    
    private void InitializeComponent()
    {
        // コントロールの初期化コード
        txtName = new TextBox();
        txtEmail = new TextBox();
        dtpBirthDate = new DateTimePicker();
        btnNext = new Button();
        btnCancel = new Button();
        
        // レイアウトとプロパティの設定
        // ...
    }
    
    public override object SaveState()
    {
        return new { 
            Name = txtName.Text, 
            Email = txtEmail.Text,
            BirthDate = dtpBirthDate.Value
        };
    }
    
    public override void RestoreState(object state)
    {
        dynamic data = state;
        txtName.Text = data.Name;
        txtEmail.Text = data.Email;
        dtpBirthDate.Value = data.BirthDate;
    }
    
    public override bool Validate()
    {
        // 入力検証ロジック
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("名前を入力してください。");
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@"))
        {
            MessageBox.Show("有効なメールアドレスを入力してください。");
            return false;
        }
        
        return true;
    }
}
```

## このアーキテクチャのメリット

1. **宣言的なフロー定義**: フローを事前に定義することで、画面遷移ロジックとUI実装を分離できます。
2. **再利用可能**: 同じ画面を異なるフローで再利用できます。
3. **メンテナンス性**: 画面の追加や順序変更が容易になります。
4. **状態管理**: 各画面の状態が適切に保存・復元されます。
5. **ユーザビリティ**: パンくずリストによる現在位置の表示と直接ナビゲーションが可能です。

この実装では、フローの定義を単純な設定ファイルではなく、コードベースで行っていますが、必要に応じてXMLやJSONなどの設定ファイルからフロー定義を読み込む機能も追加できます。

より詳細な実装や特定の要件に対するカスタマイズが必要でしたら、お知らせください。

---

了解です。「一気に戻る」とは、**指定ページまで一気に戻り、そのページを表示し、それ以降の履歴はすべて破棄する**という動作ですね。  
以下のように強制的に**対象ページをトップにして**遷移するメソッドを用意できます。

---

## ✅ メソッド名案

```csharp
public void JumpBackTo<TTarget>(params object[] aTempData) where TTarget : UserControl
```

---

## ✅ 実装（履歴を削って一気に戻る）

```csharp
public void JumpBackTo<TTarget>(params object[] aTempData) where TTarget : UserControl
{
    Type targetType = typeof(TTarget);

    if (!_history.Contains(targetType))
        return;

    // 対象が出てくるまで全部削除（対象は除く）
    while (_history.Count > 0 && _history.Peek() != targetType)
    {
        _history.Pop();
    }

    if (_history.Count == 0)
        return;

    // 遷移処理
    var from = _currentPage?.GetType();
    var to = targetType;

    var uc_from = _currentPage;
    var uc_to = (UserControl)_provider.Resolve(to);

    Context.TempData = aTempData;
    Context.DefaultNextPage = to;
    Context.NextPage = to;

    InternalNavigateTo(uc_from, uc_to, null, from, to);
}
```

---

## ✅ 使用例

```csharp
// 途中ページをすっ飛ばして、StartPage にジャンプバックする
navigationFlowService.JumpBackTo<StartPage>("戻る理由などを渡す");
```

---

## 🔁 違いの整理

| メソッド名         | 動作                                         |
|------------------|--------------------------------------------|
| `GoPrev()`       | 1つ前に戻る                                 |
| `GoBackTo<T>()`  | 指定ページまで戻る（途中ページ残す）         |
| `JumpBackTo<T>()`| 指定ページまで一気に戻る（途中ページすべて破棄）|

---

## 💡拡張案

- `JumpBackTo<T>(..., bool includeTarget = true)` として、対象ページも履歴から除外するオプション。
- `JumpBackTo(Type targetType)` のオーバーロードを作り、動的な型指定に対応。
- 戻る前に「保存されていない変更があります。戻ってもよいですか？」のようなガードを入れたい場合は、`INavigationGuard` インターフェースでページ側にキャンセル権限を持たせると良いです。

---

必要であれば、「今どこにいるか」「履歴をUIに表示する」「ページタイトルやアイコンを持たせる」なども拡張できます！続けますか？

では NavigationResult を導入し、OnCancel / OnComplete からの戻り値として使えるように拡張します。


対象ページ（特定の `UserControl`）まで「バック」する機能を、現在の `NavigationFlowService` に追加する方法を以下に示します。

---

## ✅ 目的

- ナビゲーション履歴 `_history` にスタックされているページのうち、指定したページ `TTarget` が見つかるまで戻る。
- 見つかったらそのページへ遷移し、それ以降の履歴を破棄する。

---

## ✅ 実装例：`GoBackTo<TTarget>()`

```csharp
public void GoBackTo<TTarget>(params object[] aTempData) where TTarget : UserControl
{
    Type targetType = typeof(TTarget);

    if (_history.Count <= 1 || !_history.Contains(targetType))
        return;

    // 戻る対象が見つかるまでPopする（直近のTTargetまで）
    while (_history.Count > 0)
    {
        var popped = _history.Pop();

        if (popped == targetType)
        {
            _history.Push(popped); // TTargetは戻す（表示対象なので）
            var from = _currentPage?.GetType();
            var to = targetType;

            var uc_from = _currentPage;
            var uc_to = (UserControl)_provider.Resolve(to);

            Context.TempData = aTempData;
            Context.DefaultNextPage = to;
            Context.NextPage = to;

            InternalNavigateTo(uc_from, uc_to, null, from, to);
            return;
        }
    }
}
```

---

## ✅ 使用例

```csharp
// ユーザーが設定画面に遷移した後、フォーム入力完了後に最初の入力画面へ戻る場合など
navigationFlowService.GoBackTo<StartInputPage>();
```

---

## ✅ 拡張ポイント（任意）

1. **履歴の中に同一ページが複数ある場合の動作切り替え**（最初の一致か最後の一致か）。
2. **見つからなかった場合の通知（イベントや戻り値）**。
3. **ナビゲーション履歴を一覧で取得する機能**（デバッグやログ用）。

---

## ✅ 補足：履歴可視化（オプション）

履歴のデバッグ用に以下のようなメソッドもあると便利です：

```csharp
public IEnumerable<Type> GetHistorySnapshot()
{
    return _history.Reverse(); // 先頭が最初に遷移したページ
}
```

---

必要であれば、**履歴の状態を保存・復元**（セッション引き継ぎやスナップショット的活用）する仕組みも組み込めます。  
関心ありますか？

---

✅ NavigationResult 構造体の定義
```
public struct NavigationResult
{
    public bool ShouldClose { get; set; }               // ナビゲーションを終了して画面を閉じるか
    public Type? RedirectToPage { get; set; }           // 終了後、遷移すべきページ（nullで何もしない）
    public object[]? TempData { get; set; }             // 次ページへ渡すデータ（RedirectToPageがある場合）

    public static NavigationResult Close() => new() { ShouldClose = true };
    public static NavigationResult None() => new() { ShouldClose = false };
    public static NavigationResult Redirect<T>(params object[] data) => new()
    {
        RedirectToPage = typeof(T),
        TempData = data
    };
}
```

---

✅ NavigationFlowService に組み込み

OnCancel / OnComplete の型を変更
```
public Func<NavigationContext, NavigationResult> OnCancel;
public Func<NavigationContext, NavigationResult> OnComplete;
```
Cancel メソッドの修正
```
public void Cancel(params object[] aTempData)
{
    Context.TempData = aTempData;
    var result = OnCancel(Context);

    if (result.ShouldClose)
    {
        _container.Controls.Clear();
        _history.Clear();
    }

    if (result.RedirectToPage != null)
    {
        var uc_from = _currentPage;
        var uc_to = (UserControl)_provider.Resolve(result.RedirectToPage);
        InternalNavigateTo(uc_from, uc_to, _currentPage?.GetType(), _currentPage?.GetType(), result.RedirectToPage);
    }
}
```
Complete メソッドの修正
```
public void Complete(params object[] aTempData)
{
    Context.TempData = aTempData;
    var result = OnComplete(Context);

    if (result.ShouldClose)
    {
        _container.Controls.Clear();
        _history.Clear();
    }

    if (result.RedirectToPage != null)
    {
        var uc_from = _currentPage;
        var uc_to = (UserControl)_provider.Resolve(result.RedirectToPage);
        InternalNavigateTo(uc_from, uc_to, _currentPage?.GetType(), _currentPage?.GetType(), result.RedirectToPage);
    }
}
```

---

✅ 使用例（呼び出し側）

```
var flow = new NavigationFlowService(host, provider,
    context =>
    {
        // Cancel 時に確認ダイアログなどを出す
        var result = MessageBox.Show("キャンセルしてもよいですか？", "確認", MessageBoxButtons.YesNo);
        return result == DialogResult.Yes ? NavigationResult.Close() : NavigationResult.None();
    },
    context =>
    {
        // Complete 時に別画面へリダイレクトも可能
        return NavigationResult.Redirect<FinishPage>("タスク完了しました");
    });
```

---

拡張案

NavigationResult に ErrorMessage や ShouldShowDialog を追加して、エラー制御や UI 表示と統合も可能。

RedirectToPage が null のときは ShouldClose = true でも 再確認 を要求するオプションを追加してもよいです。



---

他にも、INavigationGuard インターフェースで OnPageLeave をキャンセル可能にするなどのルール制御も追加可能です。続けて見たいですか？




ナビゲーションフローの拡張と改善がしやすい良い設計になっています。以下の点を踏まえて、拡張・改善案と修正案を提案します。


---

✅ 不足している/改善すべき点

1. GoNext<T> / GoPrev 内の変数未定義

InternalNavigateTo(uc_from, uc_to, prev, from, to);

このままだと uc_from, uc_to, prev, from, to が定義されていないため ビルドエラーになります。

2. GoPrev が履歴だけ使っているが、実際の前ページに戻っていない


---

🔧 修正案：GoNext / GoPrev 実装
```
public void GoNext<T>(params object[] aTempData) where T : UserControl
{
    var from = _currentPage?.GetType();
    var to = typeof(T);
    var prev = _history.Count > 0 ? _history.Peek() : null;

    Context.TempData = aTempData;
    Context.DefaultNextPage = to;
    Context.NextPage = to;

    var uc_from = _currentPage;
    var uc_to = (UserControl)_provider.Resolve(to);

    _history.Push(to);
    InternalNavigateTo(uc_from, uc_to, prev, from, to);
}

public void GoPrev(params object[] aTempData)
{
    if (_history.Count <= 1) return;

    _history.Pop(); // 現在のページを捨てる
    var to = _history.Peek(); // 一つ前を取得

    var from = _currentPage?.GetType();
    var prev = _history.Count > 1 ? _history.ToArray()[1] : null;

    Context.TempData = aTempData;
    Context.DefaultNextPage = to;
    Context.NextPage = to;

    var uc_from = _currentPage;
    var uc_to = (UserControl)_provider.Resolve(to);

    InternalNavigateTo(uc_from, uc_to, prev, from, to);
}
```

---

💡 拡張案

1. INavigationResult でキャンセル／完了後の制御を明確に

OnCancel / OnComplete の戻り値が bool だけだと拡張しづらいため、以下のような構造体にすると今後の拡張が簡単です：

public struct NavigationResult
{
    public bool ShouldClose;
    public Type? RedirectToPage;
    public object[]? TempData;
}

2. 戻れるページかどうか判定用プロパティ追加

public bool CanGoBack => _history.Count > 1;

3. 履歴全削除／途中から戻る（Undo風）のメソッド

public void GoBackTo<TTarget>() where TTarget : UserControl
{
    while (_history.Count > 1)
    {
        var type = _history.Pop();
        if (type == typeof(TTarget))
        {
            GoNext<TTarget>();
            return;
        }
    }
}


---

✅ まとめ

◾ 修正ポイント

GoNext, GoPrev で未定義変数を使っている → 適切に Resolve して InternalNavigateTo に渡す。

履歴スタックを適切に活用するよう修正。


◾ 拡張案

NavigationResult による制御強化

CanGoBack プロパティ

履歴を利用した GoBackTo<T>()



---

必要であれば、このナビゲーションに INavigationGuard 的な条件付き遷移（確認ダイアログなど）を追加する案も提示できます。興味ありますか？




改良版 RegisterFormNavigationService

現在のページ型・遷移先のページ型・シェアデータを受け取る 条件分岐ルール を設定し、戻る際にも適用できるように RegisterFormNavigationService を拡張します。

新しい要件

✅ 動的な条件分岐

Next() の際、現在のページ型・遷移先のページ型・シェアデータを考慮して次の画面を決定 ✅ 戻る際の条件分岐

Previous() でも条件分岐を適用し、特定の画面をスキップ可能 ✅ 登録画面をスキップ

例: 登録画面を通過後、すでに登録されていれば戻る際も登録画面をスキップ ✅ カスタムルールの設定

ユーザーが独自のナビゲーションルールを追加可能 (RegisterNavigationRule())



---

改良後の RegisterFormNavigationService
```
using System;
using System.Collections.Generic;
using System.Windows.Forms;

public class RegisterFormNavigationService : NavigationService
{
    private Stack<Type> _history = new();
    private Action<object> _onComplete;
    private object _formData;
    private Dictionary<(Type current, Type next), Func<object, Type>> _navigationRules = new();
    private Dictionary<(Type current, Type previous), Func<object, Type>> _backNavigationRules = new();

    public RegisterFormNavigationService(Panel container, object sharedData, Action<object> onComplete)
        : base(container, sharedData)
    {
        _onComplete = onComplete;
        _formData = new Dictionary<string, object>();
    }

    public void Start<T>() where T : UserControl
    {
        _history.Clear();
        Navigate(typeof(T));
    }

    public void RegisterNavigationRule<TCurrent, TNext>(Func<object, Type> rule)
        where TCurrent : UserControl
        where TNext : UserControl
    {
        _navigationRules[(typeof(TCurrent), typeof(TNext))] = rule;
    }

    public void RegisterBackNavigationRule<TCurrent, TPrevious>(Func<object, Type> rule)
        where TCurrent : UserControl
        where TPrevious : UserControl
    {
        _backNavigationRules[(typeof(TCurrent), typeof(TPrevious))] = rule;
    }

    public void Next(Type nextPageType)
    {
        if (!typeof(UserControl).IsAssignableFrom(nextPageType))
            throw new ArgumentException("Next page must be a UserControl.");

        Type currentPageType = _currentPage?.GetType();

        if (_navigationRules.TryGetValue((currentPageType, nextPageType), out var rule))
        {
            nextPageType = rule(_sharedData);
        }

        _history.Push(currentPageType);
        Navigate(nextPageType);
    }

    public void Previous()
    {
        if (_history.Count > 0)
        {
            Type previousPage = _history.Pop();
            Type currentPageType = _currentPage?.GetType();

            if (_backNavigationRules.TryGetValue((currentPageType, previousPage), out var rule))
            {
                previousPage = rule(_sharedData);
            }

            Navigate(previousPage);
        }
    }

    public void Cancel()
    {
        if (_history.Count > 0)
        {
            Type firstPage = _history.ToArray()[^1];
            _history.Clear();
            Navigate(firstPage);
        }
    }

    public void Complete()
    {
        _onComplete?.Invoke(_formData);
    }

    public void SaveData(string key, object value)
    {
        if (_formData is Dictionary<string, object> data)
        {
            data[key] = value;
        }
    }

    public object GetData(string key)
    {
        if (_formData is Dictionary<string, object> data && data.ContainsKey(key))
        {
            return data[key];
        }
        return null;
    }
}
```

---

使用例

(1) 画面の登録
```
_navigationService.RegisterStep<Step1Page>();
_navigationService.RegisterStep<RegistrationPage>();
_navigationService.RegisterStep<ConfirmationPage>();
_navigationService.RegisterStep<CompletePage>();
```

---

(2) Next() の際のルールを設定

登録ページ (RegistrationPage) を通過後、すでに登録済みならスキップ

```
_navigationService.RegisterNavigationRule<Step1Page, RegistrationPage>((sharedData) =>
{
    bool isRegistered = (bool)_navigationService.GetData("IsRegistered");
    return isRegistered ? typeof(ConfirmationPage) : typeof(RegistrationPage);
});
```

---

(3) Previous() の際のルールを設定

ConfirmationPage から戻る際、 すでに登録されていたら Step1Page まで戻る


_navigationService.RegisterBackNavigationRule<ConfirmationPage, RegistrationPage>((sharedData) =>
{
    bool isRegistered = (bool)_navigationService.GetData("IsRegistered");
    return isRegistered ? typeof(Step1Page) : typeof(RegistrationPage);
});


---

(4) 画面遷移時の動作

✅ Step1Page から Next()

未登録 → RegistrationPage

登録済み → ConfirmationPage


✅ ConfirmationPage から Previous()

登録済み → Step1Page

未登録 → RegistrationPage



---

より柔軟なカスタマイズが可能に！


---

結論

この実装により、「登録画面を一度通過したら、もう戻らない」 といった動的ナビゲーションが可能になります。
特に、ユーザーの選択や状態によって画面フローが変わるアプリケーション に最適です！ 🚀




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


