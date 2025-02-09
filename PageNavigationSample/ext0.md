入力フォームの順番が固定されていない場合、選択した条件によって遷移先を動的に変更できる **`RegisterFormNavigationService`** を設計します。

## **要件**
1. **動的な画面遷移**
   - `Next(Type nextPageType)` で次の画面を指定して遷移
   - `Previous()` で直前の画面に戻る
2. **キャンセル機能**
   - `Cancel()` で最初の画面へリセット
3. **完了機能**
   - `Complete()` でデータを確定して完了時のコールバックを実行
4. **入力データ管理**
   - 各画面の入力データを `SaveData(key, value)` で保存・取得
5. **履歴管理**
   - `Stack<Type>` を使用し、戻る操作 (`Previous()`) に対応

---

## **RegisterFormNavigationService の実装**
```csharp
using System;
using System.Collections.Generic;
using System.Windows.Forms;

public class RegisterFormNavigationService : NavigationService
{
    private Stack<Type> _history = new(); // 画面の履歴
    private Action<object> _onComplete; // 完了時のコールバック
    private object _formData; // 入力データ

    public RegisterFormNavigationService(Panel container, object sharedData, Action<object> onComplete)
        : base(container, sharedData)
    {
        _onComplete = onComplete;
        _formData = new Dictionary<string, object>(); // 入力データ用
    }

    public void Start<T>() where T : UserControl
    {
        _history.Clear();
        Navigate(typeof(T));
    }

    public void Next(Type nextPageType)
    {
        if (!typeof(UserControl).IsAssignableFrom(nextPageType))
            throw new ArgumentException("Next page must be a UserControl.");

        _history.Push(_currentPage?.GetType());
        Navigate(nextPageType);
    }

    public void Previous()
    {
        if (_history.Count > 0)
        {
            Type previousPage = _history.Pop();
            Navigate(previousPage);
        }
    }

    public void Cancel()
    {
        if (_history.Count > 0)
        {
            Type firstPage = _history.ToArray()[^1]; // 履歴の最初のページ
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

## **画面 (`UserControl`) の実装**
### **Step1Page (条件を選択する画面)**
```csharp
using System;
using System.Windows.Forms;

public partial class Step1Page : UserControl
{
    private RegisterFormNavigationService _navService;

    public Step1Page(RegisterFormNavigationService navService)
    {
        InitializeComponent();
        _navService = navService;
    }

    private void btnNext_Click(object sender, EventArgs e)
    {
        _navService.SaveData("Step1", textBoxInput.Text);

        // ユーザーの入力によって次の画面を動的に決定
        if (radioOption1.Checked)
        {
            _navService.Next(typeof(Option1Page));
        }
        else if (radioOption2.Checked)
        {
            _navService.Next(typeof(Option2Page));
        }
    }
}
```

---

### **Option1Page (選択肢1 の場合の画面)**
```csharp
using System;
using System.Windows.Forms;

public partial class Option1Page : UserControl
{
    private RegisterFormNavigationService _navService;

    public Option1Page(RegisterFormNavigationService navService)
    {
        InitializeComponent();
        _navService = navService;
    }

    private void btnPrevious_Click(object sender, EventArgs e)
    {
        _navService.Previous();
    }

    private void btnComplete_Click(object sender, EventArgs e)
    {
        _navService.SaveData("Option1", textBoxInput.Text);
        _navService.Complete();
    }
}
```

---

### **Option2Page (選択肢2 の場合の画面)**
```csharp
using System;
using System.Windows.Forms;

public partial class Option2Page : UserControl
{
    private RegisterFormNavigationService _navService;

    public Option2Page(RegisterFormNavigationService navService)
    {
        InitializeComponent();
        _navService = navService;
    }

    private void btnPrevious_Click(object sender, EventArgs e)
    {
        _navService.Previous();
    }

    private void btnComplete_Click(object sender, EventArgs e)
    {
        _navService.SaveData("Option2", textBoxInput.Text);
        _navService.Complete();
    }
}
```

---

## **フォーム側 (`Form1`) の実装**
```csharp
using System;
using System.Windows.Forms;

public partial class Form1 : Form
{
    private RegisterFormNavigationService _navigationService;

    public Form1()
    {
        InitializeComponent();
        _navigationService = new RegisterFormNavigationService(
            panelContainer, "Shared Data Example", OnFormCompleted);

        // フォーム開始（最初の画面を指定）
        _navigationService.Start<Step1Page>();
    }

    private void OnFormCompleted(object formData)
    {
        if (formData is Dictionary<string, object> data)
        {
            string result = "Form Completed!\n";
            foreach (var kv in data)
            {
                result += $"{kv.Key}: {kv.Value}\n";
            }
            MessageBox.Show(result);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _navigationService?.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

---

## **動作フロー**
1. **Step1Page で条件を選択**
   - `Next(typeof(Option1Page))` もしくは `Next(typeof(Option2Page))`
2. **Option1Page or Option2Page へ遷移**
   - `Previous()` で `Step1Page` に戻る
   - `Complete()` でフォーム完了
3. **履歴管理**
   - `_history` に遷移履歴を保存し、`Previous()` で前の画面に戻れる
4. **キャンセル**
   - `Cancel()` を呼ぶと、最初の画面へリセット

---

## **特徴**
✅ **動的遷移:** `Next(Type nextPageType)` により、ユーザーの選択によって遷移先を変えられる  
✅ **戻る機能:** `_history` を利用し、`Previous()` で履歴に沿って戻れる  
✅ **データ管理:** `SaveData(key, value)` でフォームデータを保存  
✅ **キャンセル機能:** `Cancel()` で最初の画面に戻る  
✅ **完了機能:** `Complete()` で全データを収集し、処理を実行  

これで、**入力内容に応じて動的に遷移する複数画面フォーム** を WinForms で実装できます！



### **`NavigationService` や `RegisterFormNavigationService` に追加すると便利な機能**
現在の実装でも十分な機能を備えていますが、以下のような機能を追加するとさらに柔軟なナビゲーションやフォーム入力が可能になります。

---

## **1. NavigationService の拡張**
### ✅ **(1) 画面遷移イベントの追加**
- `Navigated` イベントを追加し、画面遷移時にフックできるようにする
- UIの更新やログ出力に活用

**追加コード**
```csharp
public event Action<Type, object> Navigated;

public override void Navigate<T>(object tempData = null)
{
    base.Navigate<T>(tempData);
    Navigated?.Invoke(typeof(T), tempData);
}
```

---

### ✅ **(2) 画面スタックのクリア**
- `ClearHistory()` を追加し、戻る履歴をリセット
- `Previous()` できなくする（特定の画面からは戻れないようにする）

**追加コード**
```csharp
public void ClearHistory()
{
    _history.Clear();
}
```

---

### ✅ **(3) グローバルな `Back()` をサポート**
- `Previous()` とは別に、 **「共通の戻るボタン」** に対応
- 特定の画面にいたら特別な処理を実行（例: 最後の画面では「終了」する）

**追加コード**
```csharp
public void Back()
{
    if (_history.Count > 0)
    {
        Previous();
    }
    else
    {
        Application.Exit(); // または初期画面へ
    }
}
```

---

## **2. RegisterFormNavigationService の拡張**
### ✅ **(4) ステップ表示 (プログレスバー)**
- 現在の進捗を把握できるようにする (`CurrentStep` / `TotalSteps` に加えて進捗通知)

**追加コード**
```csharp
public event Action<int, int> ProgressUpdated;

public override void Next(Type nextPageType)
{
    base.Next(nextPageType);
    ProgressUpdated?.Invoke(CurrentStep, TotalSteps);
}
```
**使用例**
```csharp
_navService.ProgressUpdated += (step, total) => progressBar.Value = (step * 100) / total;
```

---

### ✅ **(5) 途中保存・復元機能**
- 途中まで入力したデータを保存し、アプリ再起動後も継続できる
- `SaveState()` / `LoadState()` でフォームデータをファイルに保存・復元

**追加コード**
```csharp
using System.IO;
using System.Text.Json;

public void SaveState(string filePath)
{
    File.WriteAllText(filePath, JsonSerializer.Serialize(_formData));
}

public void LoadState(string filePath)
{
    if (File.Exists(filePath))
    {
        _formData = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(filePath));
    }
}
```

---

### ✅ **(6) 条件分岐のナビゲーションルール**
- `Next()` で次の画面を決めるだけでなく、 **「特定の条件ならスキップ」** などのルールを定義できるようにする

**追加コード**
```csharp
private Dictionary<Type, Func<bool>> _skipConditions = new();

public void RegisterSkipCondition<T>(Func<bool> condition)
{
    _skipConditions[typeof(T)] = condition;
}

public override void Next(Type nextPageType)
{
    while (_skipConditions.TryGetValue(nextPageType, out var condition) && condition())
    {
        // 次のページをスキップしてさらに次へ
        nextPageType = GetNextPageAfter(nextPageType);
    }
    base.Next(nextPageType);
}
```

**使用例**
```csharp
_navService.RegisterSkipCondition<Option2Page>(() => (bool)_navService.GetData("SkipOption2"));
```

---

### ✅ **(7) タイムアウトでフォーム入力を自動キャンセル**
- 長時間操作がない場合、フォームをキャンセルして最初からやり直す

**追加コード**
```csharp
private System.Timers.Timer _timeoutTimer;

public void EnableTimeout(TimeSpan duration)
{
    _timeoutTimer = new System.Timers.Timer(duration.TotalMilliseconds);
    _timeoutTimer.Elapsed += (s, e) => Cancel();
    _timeoutTimer.Start();
}

public void ResetTimeout()
{
    _timeoutTimer?.Stop();
    _timeoutTimer?.Start();
}
```

**使用例**
```csharp
_navService.EnableTimeout(TimeSpan.FromMinutes(5));
```
---

### ✅ **(8) ユーザーが操作したデータのトラッキング**
- どの画面で何を入力したかログを記録する
- `OnNavigated()` にトラッキング機能を組み込む

**追加コード**
```csharp
public event Action<Type, string, object> DataTracked;

public override void SaveData(string key, object value)
{
    base.SaveData(key, value);
    DataTracked?.Invoke(_currentPage?.GetType(), key, value);
}
```

**使用例**
```csharp
_navService.DataTracked += (page, key, value) => Console.WriteLine($"{page.Name} - {key}: {value}");
```

---

### ✅ **(9) 入力チェック（バリデーション）**
- 各画面で「次へ」進む前にバリデーションを行う
- `Next()` の前に `ValidateCurrentPage()` を実行

**追加コード**
```csharp
public bool ValidateCurrentPage()
{
    if (_currentPage is IValidatablePage validatable)
    {
        return validatable.Validate();
    }
    return true; // バリデーションなしの場合はOK
}

public override void Next(Type nextPageType)
{
    if (ValidateCurrentPage())
    {
        base.Next(nextPageType);
    }
}
```

**使用例 (`UserControl` 側)**
```csharp
public interface IValidatablePage
{
    bool Validate();
}

public partial class Step1Page : UserControl, IValidatablePage
{
    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(textBoxInput.Text);
    }
}
```

---

## **拡張機能のまとめ**
| 機能 | 説明 |
|------|------|
| **ナビゲーションイベント** | 画面遷移時にフックできる (`Navigated`) |
| **履歴クリア** | `ClearHistory()` で戻れないようにする |
| **共通の `Back()`** | 戻る履歴がない場合はアプリ終了 |
| **プログレスバー更新** | `ProgressUpdated` で進捗バーを連動 |
| **途中保存・復元** | `SaveState()` / `LoadState()` で中断対応 |
| **スキップ条件** | `RegisterSkipCondition<T>()` でページをスキップ |
| **自動キャンセル (タイムアウト)** | `EnableTimeout()` で放置時にリセット |
| **データトラッキング** | `DataTracked` で入力履歴を記録 |
| **入力チェック** | `ValidateCurrentPage()` でバリデーション |

---

## **結論**
- **動的なフォーム遷移** に対応しつつ、 **ユーザーの操作をよりスムーズにする機能** を追加
- **バリデーション・途中保存・進捗表示** など、実際の業務システムにも適用しやすい
- **ナビゲーションの制御を細かくできる** ため、複雑な入力フローでも柔軟に対応可能

これらを追加すると、**WinForms でも高度なナビゲーションを持つ入力フォームを実装できる！** 🚀