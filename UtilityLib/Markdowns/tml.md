


ありがとうございます、構成がよく整理されていますね。以下のようなクラス構成を提案します。C# / WinForms環境を想定しています。


---

1. ISnapshotable<T> インターフェース
,```
public interface ISnapshotable<T>
{
    T CreateSnapshot();
    void RestoreFromSnapshot(T snapshot);
}
```

---
```
2. 登録フローデータクラス (RegistrationFlowData)

梱包（Package）リストを保持。

public class RegistrationFlowData : ISnapshotable<RegistrationFlowData>
{
    public List<Package> Packages { get; set; } = new();

    public RegistrationFlowData CreateSnapshot()
    {
        return new RegistrationFlowData
        {
            Packages = Packages.Select(p => p.CreateSnapshot()).ToList()
        };
    }

    public void RestoreFromSnapshot(RegistrationFlowData snapshot)
    {
        Packages.Clear();
        foreach (var pkg in snapshot.Packages)
        {
            var copy = new Package();
            copy.RestoreFromSnapshot(pkg);
            Packages.Add(copy);
        }
    }
}

```
---
```
3. 梱包クラス (Package)

HierarchicalBase, IHierarchicalRoot 継承。箱ページモデルリストを保持。

public class Package : HierarchicalBase, IHierarchicalRoot, ISnapshotable<Package>
{
    public List<BoxPageModel> BoxPages { get; set; } = new();

    // IHierarchicalRootの実装
    public event EventHandler? DescendantAttached;
    public event EventHandler? DescendantDetached;

    public void OnDescendantAttached(IHierarchical descendant) => DescendantAttached?.Invoke(this, EventArgs.Empty);
    public void OnDescendantDetached(IHierarchical descendant) => DescendantDetached?.Invoke(this, EventArgs.Empty);

    public Package CreateSnapshot()
    {
        return new Package
        {
            BoxPages = BoxPages.Select(p => p.CreateSnapshot()).ToList()
        };
    }

    public void RestoreFromSnapshot(Package snapshot)
    {
        BoxPages.Clear();
        foreach (var page in snapshot.BoxPages)
        {
            var restored = new BoxPageModel();
            restored.RestoreFromSnapshot(page);
            BoxPages.Add(restored);
        }
    }
}

```
---
```
4. 箱ページモデル (BoxPageModel)

public class BoxPageModel : ISnapshotable<BoxPageModel>
{
    public PageContext Context { get; set; } = new();
    public List<string> SerialNumbers { get; set; } = new(); // または専用SerialNumberクラス

    public BoxPageModel CreateSnapshot()
    {
        return new BoxPageModel
        {
            Context = Context.Clone(), // Cloneを用意
            SerialNumbers = SerialNumbers.ToList()
        };
    }

    public void RestoreFromSnapshot(BoxPageModel snapshot)
    {
        Context = snapshot.Context.Clone();
        SerialNumbers = snapshot.SerialNumbers.ToList();
    }
}

```
---

5. 拡張案

ISnapshotable<T> に bool HasChangedSinceSnapshot(T snapshot) を追加して変更検知。

スナップショットを JSON などで保存／復元する SnapshotSerializer<T> クラス。

Undo/Redo 対応として HistoryManager<T> を導入。

WinForms UI上でスナップショット状態との比較ビュー。



---

必要に応じて、HierarchicalBase や PageContext の実装も用意できます。さらに拡張したい点があれば教えてください。

了解です。親子関係（階層構造）を含んだスナップショットを取り、Undo可能にするには、以下の要点を押さえる必要があります：


---

■ 要件整理

1. スナップショット対象：
　RegistrationFlowData から Package → BoxPageModel → SerialNumbers と親子関係あり。


2. 階層構造の復元：
　スナップショット復元時に SetParent や NotifyAttachedToHierarchy などで親子関係を再構築。


3. Undo/Redo の仕組み：
　ISnapshotable<T> + HistoryManager<T> を用意して過去の状態に戻せるようにする。




---

■ 改良版インターフェース

public interface ISnapshotable<T>
{
    T CreateSnapshot();
    void RestoreFromSnapshot(T snapshot);
}


---

■ Undo用 HistoryManager<T>

public class HistoryManager<T> where T : ISnapshotable<T>
{
    private readonly Stack<T> _undoStack = new();
    private readonly Stack<T> _redoStack = new();
    private readonly T _target;

    public HistoryManager(T target)
    {
        _target = target;
    }

    public void TakeSnapshot()
    {
        _undoStack.Push(_target.CreateSnapshot());
        _redoStack.Clear(); // Redoの履歴はリセット
    }

    public void Undo()
    {
        if (_undoStack.Count > 0)
        {
            var snapshot = _undoStack.Pop();
            _redoStack.Push(_target.CreateSnapshot());
            _target.RestoreFromSnapshot(snapshot);
        }
    }

    public void Redo()
    {
        if (_redoStack.Count > 0)
        {
            var snapshot = _redoStack.Pop();
            _undoStack.Push(_target.CreateSnapshot());
            _target.RestoreFromSnapshot(snapshot);
        }
    }
}


---

■ 階層構造復元付き Restore（Package例）

public class Package : HierarchicalBase, IHierarchicalRoot, ISnapshotable<Package>
{
    public List<BoxPageModel> BoxPages { get; set; } = new();

    // スナップショット用クローン
    public Package CreateSnapshot()
    {
        var snapshot = new Package();
        foreach (var page in BoxPages)
        {
            var copy = page.CreateSnapshot();
            snapshot.BoxPages.Add(copy);
        }
        return snapshot;
    }

    public void RestoreFromSnapshot(Package snapshot)
    {
        BoxPages.Clear();

        foreach (var snapPage in snapshot.BoxPages)
        {
            var newPage = new BoxPageModel();
            newPage.RestoreFromSnapshot(snapPage);
            newPage.SetParent(this); // 親子関係を再構築
            BoxPages.Add(newPage);
        }

        // ルートイベントを再通知
        foreach (var child in BoxPages)
        {
            NotifyAttachedToHierarchy(new HierarchyAttachmentEventArgs(this));
        }
    }

    // IHierarchicalRootのイベント
    public event EventHandler? DescendantAttached;
    public event EventHandler? DescendantDetached;

    public void OnDescendantAttached(IHierarchical descendant) => DescendantAttached?.Invoke(this, EventArgs.Empty);
    public void OnDescendantDetached(IHierarchical descendant) => DescendantDetached?.Invoke(this, EventArgs.Empty);
}


---

■ 登録フローデータでも同様に

public class RegistrationFlowData : ISnapshotable<RegistrationFlowData>
{
    public List<Package> Packages { get; set; } = new();

    public RegistrationFlowData CreateSnapshot()
    {
        var snapshot = new RegistrationFlowData();
        snapshot.Packages = Packages.Select(p => p.CreateSnapshot()).ToList();
        return snapshot;
    }

    public void RestoreFromSnapshot(RegistrationFlowData snapshot)
    {
        Packages.Clear();
        foreach (var snapPackage in snapshot.Packages)
        {
            var newPackage = new Package();
            newPackage.RestoreFromSnapshot(snapPackage);
            Packages.Add(newPackage);
        }
    }
}


---

■ 拡張案

1. 差分ベーススナップショット：変更箇所だけ保存して軽量化。


2. イベント通知付きスナップショット：復元時にUI通知。


3. Version付きSnapshot：バージョン管理でUIに巻き戻し履歴を表示。




---

必要であれば IHierarchical, HierarchicalBase, BoxPageModel などのコードもあわせて提示できます。どの階層でUndo/Redoを使いたいか（例：全体かパッケージ単位か）も教えてもらえればさらに絞れます！




DataAnnotations ベースの IValidator 実装例を以下に示します。System.ComponentModel.DataAnnotations を使って属性に基づくバリデーションを行う方式です。


---

1. DataAnnotationsValidator 実装

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class DataAnnotationsValidator : IValidator
{
    private readonly object _instance;

    public DataAnnotationsValidator(object instance)
    {
        _instance = instance;
    }

    public IEnumerable<string> Validate(string propertyName, object? value)
    {
        var context = new ValidationContext(_instance)
        {
            MemberName = propertyName
        };

        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateProperty(value, context, results);

        foreach (var result in results)
        {
            yield return result.ErrorMessage ?? "不明なエラー";
        }
    }
}


---

2. モデルに属性を付ける

ObservableIModel を継承したモデルに属性を付与します。

using System.ComponentModel.DataAnnotations;

public class PersonModel : ObservableIModel
{
    public PersonModel() : base(new DataAnnotationsValidator(this)) { }

    private string _name = string.Empty;

    [Required(ErrorMessage = "名前は必須です。")]
    [StringLength(50, ErrorMessage = "名前は50文字以内で入力してください。")]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private int _age;

    [Range(0, 120, ErrorMessage = "年齢は0〜120の範囲で指定してください。")]
    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }
}


---

ポイント

DataAnnotationsValidator は ValidationContext を通して、object のインスタンスに属するプロパティの属性を読み取ります。

属性のついたプロパティに SetProperty を通じて値をセットすると、自動的にバリデーションが実行されます。

エラーは INotifyDataErrorInfo の仕組みによって UI に反映されます（DataGridView, TextBox, ErrorProvider などで活用可能）。



---

拡張案

全プロパティ一括検証: ValidateAll() を DataAnnotationsValidator に追加可能。

複合バリデータ対応: DataAnnotationsValidator と独自バリデータを組み合わせた CompositeValidator を作成可能。

属性のローカライズ対応: ErrorMessageResourceType を使ってエラーメッセージの多言語対応。


必要があれば、ErrorProviderとの連携サンプルやWPF向けの実装にも対応できます。続きを希望しますか？





ObservableCollection<T> に格納された ObservableModel（INotifyDataErrorInfo 実装済）なアイテムのエラーチェックを行う方法は以下のようになります。


---

【目的】

ObservableCollection<T> にバインドされた複数モデルの中で、どれか1つでもエラーがあるかチェックしたい。

UIや保存処理の前に全体の検証状態を確認したい。



---

【1】コレクションの検証ヘルパー

using System.Collections.ObjectModel;
using System.ComponentModel;

public static class ValidationHelper
{
    public static bool HasErrors<T>(ObservableCollection<T> collection) where T : INotifyDataErrorInfo
    {
        return collection.Any(item => item.HasErrors);
    }

    public static void ValidateAll<T>(ObservableCollection<T> collection) where T : ObservableModel
    {
        foreach (var item in collection)
        {
            item.ValidateAllProperties();
        }
    }
}


---

【2】使用例

ObservableCollection<PersonModel> people = new()
{
    new PersonModel { Name = "田中", Age = 25 },
    new PersonModel { Name = "", Age = 999 } // ←バリデーションエラー
};

// 全件バリデート（UIが自動で反映される）
ValidationHelper.ValidateAll(people);

// エラー有無チェック
bool hasError = ValidationHelper.HasErrors(people);

if (hasError)
{
    MessageBox.Show("1つ以上の入力に誤りがあります。");
}
else
{
    // 保存処理など
}


---

【3】拡張案

イベント購読による動的エラーフラグの監視
コレクション中のモデルに対して ErrorsChanged を購読し、全体の状態をリアルタイム更新。

UI連携用のViewModelに全体エラーを保持する
例：public bool CollectionHasError => ValidationHelper.HasErrors(MyCollection);

ObservableCollection を拡張して INotifyDataErrorInfo を持たせる



---

【補足】WPFなら Validation.HasError を ItemsControl に使う手もありますが、WinFormsなら ErrorProvider との統合が一般的です。

WinFormsの DataGridView にバインドする場合など、個々の行に ErrorProvider を紐づける方法も説明可能です。必要ですか？




DataAnnotations ベースのバリデーションに対応した ObservableIModel（ここでは ObservableModel と名前変更）を、より自然な形に整理した完全コードを以下に示します。


---

【1】ObservableModel（INotifyPropertyChanged + INotifyDataErrorInfo + DataAnnotations）

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

public abstract class ObservableModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly Dictionary<string, List<string>> _errors = new();

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(propertyName);
        ValidateProperty(propertyName, value);
        return true;
    }

    protected virtual void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void ValidateProperty(string propertyName, object? value)
    {
        // エラーを一度消す
        if (_errors.ContainsKey(propertyName))
            _errors.Remove(propertyName);

        var context = new ValidationContext(this)
        {
            MemberName = propertyName
        };

        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateProperty(value, context, results);

        if (results.Count > 0)
        {
            var messages = new List<string>();
            foreach (var result in results)
            {
                messages.Add(result.ErrorMessage ?? "不明なエラー");
            }

            _errors[propertyName] = messages;
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName)) return Array.Empty<string>();
        return _errors.TryGetValue(propertyName, out var errors) ? errors : Array.Empty<string>();
    }

    /// <summary>
    /// モデル全体の一括検証（オプション）
    /// </summary>
    public void ValidateAllProperties()
    {
        var properties = GetType().GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(this);
            ValidateProperty(property.Name, value);
        }
    }
}


---

【2】モデルの例（属性でバリデーション）

using System.ComponentModel.DataAnnotations;

public class PersonModel : ObservableModel
{
    private string _name = string.Empty;

    [Required(ErrorMessage = "名前は必須です。")]
    [StringLength(50, ErrorMessage = "名前は50文字以内で入力してください。")]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private int _age;

    [Range(0, 120, ErrorMessage = "年齢は0〜120の範囲で指定してください。")]
    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }
}


---

【拡張案】

ErrorProvider と組み合わせて WinForms UI にバリデーション表示。

ValidateAllProperties() を保存前や登録前チェックに使う。

継承元クラスに [MetadataType(typeof(MetaClass))] を使ってモデル本体とは別に属性定義することも可能。


必要に応じて、WinForms画面での使い方や、ErrorProviderの連携例も提示できます。表示側の例も見ますか？





WinFormsでのSaveAs、Save、Open処理を、変更フラグ（IsModified）とファイル指定フラグ（HasFilePath）を持つ構成でまとめると、以下のようなフローになります。OpenFileDialog、SaveFileDialog、MessageBoxを使用した具体例も併せて示します。


---

フラグ定義

bool IsModified = false;
string? CurrentFilePath = null;
bool HasFilePath => !string.IsNullOrEmpty(CurrentFilePath);


---

Open のフロー

(1) 編集済みであれば保存確認 → YesならSave(), Cancelなら中断
(2) OpenFileDialogを表示し、ファイル選択
(3) ファイル読み込み、内容反映
(4) IsModified = false, CurrentFilePathを更新

public void OpenFile()
{
    if (IsModified)
    {
        var result = MessageBox.Show("変更内容を保存しますか？", "確認", MessageBoxButtons.YesNoCancel);
        if (result == DialogResult.Cancel)
            return;
        if (result == DialogResult.Yes)
            SaveFile(); // Save()はHasFilePathチェックを含む
    }

    using var dialog = new OpenFileDialog { Filter = "Text Files|*.txt|All Files|*.*" };
    if (dialog.ShowDialog() == DialogResult.OK)
    {
        var content = File.ReadAllText(dialog.FileName);
        textBox.Text = content;
        CurrentFilePath = dialog.FileName;
        IsModified = false;
    }
}


---

Save のフロー

(1) HasFilePathがtrueならそのパスに上書き保存
(2) なければ SaveAs() を呼び出す

public void SaveFile()
{
    if (HasFilePath)
    {
        File.WriteAllText(CurrentFilePath, textBox.Text);
        IsModified = false;
    }
    else
    {
        SaveFileAs();
    }
}


---

Save As のフロー

(1) SaveFileDialogを表示
(2) 選ばれたパスに保存
(3) CurrentFilePathを更新、IsModifiedをfalseに

public void SaveFileAs()
{
    using var dialog = new SaveFileDialog { Filter = "Text Files|*.txt|All Files|*.*" };
    if (dialog.ShowDialog() == DialogResult.OK)
    {
        File.WriteAllText(dialog.FileName, textBox.Text);
        CurrentFilePath = dialog.FileName;
        IsModified = false;
    }
}


---

拡張案

タイトルバーにファイル名を表示：

void UpdateTitle() => this.Text = $"MyEditor - {(HasFilePath ? Path.GetFileName(CurrentFilePath) : "無題")}{(IsModified ? "*" : "")}";

フォームクローズ時の保存確認：

private void Form_FormClosing(object sender, FormClosingEventArgs e)
{
    if (IsModified)
    {
        var result = MessageBox.Show("変更を保存しますか？", "終了確認", MessageBoxButtons.YesNoCancel);
        if (result == DialogResult.Cancel)
            e.Cancel = true;
        else if (result == DialogResult.Yes)
            SaveFile();
    }
}



---

必要であれば、これをクラス構造やMVVM風に整理するアーキテクチャも提示できます。続きを希望されますか？




以下は、INotifyPropertyChanged、INotifyDataErrorInfo、IValidator を使ってバリデーションを行い、ObservableCollection を DataGridView にバインドし、プロパティにエラーがある場合に MessageBox を表示する WinForms の例です。


---

1. バリデーション対象のモデルクラス

public class Person : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private string _name;
    private readonly Dictionary<string, List<string>> _errors = new();

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            OnPropertyChanged(nameof(Name));
            ValidateName();
        }
    }

    private void ValidateName()
    {
        ClearErrors(nameof(Name));
        if (string.IsNullOrWhiteSpace(Name))
        {
            AddError(nameof(Name), "名前は必須です。");
        }
        else if (Name.Length < 3)
        {
            AddError(nameof(Name), "名前は3文字以上で入力してください。");
        }
    }

    public bool HasErrors => _errors.Any();
    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    public IEnumerable GetErrors(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName)) return null;
        return _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;
    }

    private void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = new List<string>();
        if (!_errors[propertyName].Contains(error))
        {
            _errors[propertyName].Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    private void ClearErrors(string propertyName)
    {
        if (_errors.ContainsKey(propertyName))
        {
            _errors.Remove(propertyName);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}


---

2. IValidator の定義（オプション）

これは拡張としてモデルとは別にバリデーション戦略を分離したいときに使えます：

public interface IValidator<T>
{
    Dictionary<string, List<string>> Validate(T instance);
}

public class PersonValidator : IValidator<Person>
{
    public Dictionary<string, List<string>> Validate(Person person)
    {
        var errors = new Dictionary<string, List<string>>();
        if (string.IsNullOrWhiteSpace(person.Name))
        {
            errors[nameof(person.Name)] = new List<string> { "名前は必須です。" };
        }
        else if (person.Name.Length < 3)
        {
            errors[nameof(person.Name)] = new List<string> { "名前は3文字以上で入力してください。" };
        }
        return errors;
    }
}


---

3. フォームとバインディング

public partial class MainForm : Form
{
    private BindingList<Person> _people = new();

    public MainForm()
    {
        InitializeComponent();

        _people.ListChanged += People_ListChanged;
        dataGridView1.AutoGenerateColumns = true;
        dataGridView1.DataSource = _people;

        // テストデータ
        _people.Add(new Person { Name = "太郎" });
        _people.Add(new Person { Name = "" }); // エラーになる
    }

    private void People_ListChanged(object sender, ListChangedEventArgs e)
    {
        if (e.ListChangedType == ListChangedType.ItemChanged)
        {
            var person = _people[e.NewIndex];
            if (person is INotifyDataErrorInfo errorInfo && errorInfo.HasErrors)
            {
                var errors = errorInfo.GetErrors(nameof(Person.Name))?.Cast<string>()?.ToList();
                if (errors?.Any() == true)
                {
                    MessageBox.Show(string.Join("\n", errors), "バリデーションエラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}


---

拡張案

IValidator<T> を使って Person クラスの Validate を外部委譲に置き換える。

IDataErrorInfo にも対応することで WPF 互換の柔軟性を高める。

ErrorProvider を併用して、UI にエラー表示を追加。

Undo/Redo 機構と連携した変更履歴管理。

ValidationContext を使った DataAnnotations 互換の汎用バリデーション。



---

必要であれば ErrorProvider バージョンも出せます。続けて何を拡張しますか？




WinForms向けに、以下の要件を満たす汎用的なロード画面表示用の仕組みを提供します：

ロード画面は以下のUI要素を持つ

メッセージラベル

プログレスバー（マーキー or 値表示）

進捗ラベル（xx%のように表示）

キャンセルボタン


非同期処理実行中に表示

マーキースタイルの進捗不要版と進捗制御あり版の2パターンの実行メソッドを用意



---

1. LoadingForm クラス（共通のUI）

public partial class LoadingForm : Form
{
    private CancellationTokenSource _cts;

    public CancellationToken Token => _cts.Token;

    public LoadingForm(string message, bool useProgress)
    {
        InitializeComponent();
        lblMessage.Text = message;
        progressBar.Style = useProgress ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
        lblProgress.Text = useProgress ? "0%" : "";
        btnCancel.Visible = true;
        _cts = new CancellationTokenSource();
    }

    public void UpdateProgress(int value)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateProgress(value));
            return;
        }

        progressBar.Value = Math.Min(value, 100);
        lblProgress.Text = $"{value}%";
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        _cts.Cancel();
        btnCancel.Enabled = false;
        lblMessage.Text = "キャンセル中…";
    }
}


---

2. マーキー用：RunWithLoadingAsync メソッド

public static async Task RunWithLoadingAsync(string message, Func<CancellationToken, Task> action)
{
    using var loadingForm = new LoadingForm(message, useProgress: false);

    var task = Task.Run(() => action(loadingForm.Token));
    loadingForm.Show();

    while (!task.IsCompleted && !task.IsCanceled)
    {
        Application.DoEvents(); // フリーズ防止
        await Task.Delay(100);
    }

    loadingForm.Close();
    await task;
}


---

3. 進捗あり用：RunWithProgressLoadingAsync メソッド

public static async Task RunWithProgressLoadingAsync(string message, Func<IProgress<int>, CancellationToken, Task> action)
{
    using var loadingForm = new LoadingForm(message, useProgress: true);
    var progress = new Progress<int>(val => loadingForm.UpdateProgress(val));

    var task = Task.Run(() => action(progress, loadingForm.Token));
    loadingForm.Show();

    while (!task.IsCompleted && !task.IsCanceled)
    {
        Application.DoEvents(); // フリーズ防止
        await Task.Delay(100);
    }

    loadingForm.Close();
    await task;
}


---

使用例

マーキーモード

await RunWithLoadingAsync("処理中です...", async token =>
{
    await Task.Delay(3000, token); // 疑似処理
});

プログレスあり

await RunWithProgressLoadingAsync("データを読み込み中...", async (progress, token) =>
{
    for (int i = 0; i <= 100; i++)
    {
        token.ThrowIfCancellationRequested();
        progress.Report(i);
        await Task.Delay(50);
    }
});


---

拡張案

タイムアウト設定の追加（一定時間後に自動キャンセル）

ローディングメッセージの途中変更 (UpdateMessage メソッド追加)

エラーハンドリングUI（例外表示用のラベル or メッセージボックス）

非同期処理の中でキャンセル時のUI表示


必要であれば、LoadingForm の Designer.cs も用意できます！作りますか？



キャンセル時にユーザーへ適切なフィードバックを与えるために、キャンセルされたことを明確にUI上で表示し、必要であればすぐ閉じずに少しの間表示する、またはエラーメッセージ表示と同じように再利用できるように設計すると良いです。


---

LoadingForm におけるキャンセル時のUI変更案

変更ポイント：

1. メッセージラベルに「キャンセルされました」と表示


2. プログレスバーを停止状態に変更


3. キャンセルボタンを非活性化


4. 自動的に少し待って閉じる（オプション）




---

修正後の btnCancel_Click ハンドラー

private async void btnCancel_Click(object sender, EventArgs e)
{
    btnCancel.Enabled = false;
    _cts.Cancel();
    
    lblMessage.Text = "キャンセルされました";
    progressBar.Style = ProgressBarStyle.Blocks;
    lblProgress.Text = "";

    // 任意：ユーザーが読めるように1秒ほど待つ
    await Task.Delay(1000);

    Close();
}


---

拡張：処理側からキャンセル後のUI制御を行いたい場合

LoadingForm に次のようなメソッドを追加して、処理完了後に キャンセル済み、正常終了、エラー を切り替えて表示できます：

public void ShowStatusMessage(string message, Color color)
{
    if (InvokeRequired)
    {
        Invoke(() => ShowStatusMessage(message, color));
        return;
    }

    lblMessage.Text = message;
    lblMessage.ForeColor = color;
    progressBar.Style = ProgressBarStyle.Blocks;
    lblProgress.Text = "";
}

使用例：

loadingForm.ShowStatusMessage("キャンセルされました", Color.DarkOrange);


---

補足：キャンセル時にフォームを即時閉じず、ユーザーが確認するまで待機したい場合

キャンセルボタン押下後は閉じずに一定時間表示、または閉じボタンを表示して手動で閉じてもらうパターンも検討できます。その場合は Close() 呼び出しを外して、代わりにフォーム右上で閉じるか「閉じる」ボタンを新たに追加して対応します。


---

ご希望に合わせて「キャンセル後に自動再試行」「リトライボタンを表示」なども組み込み可能です。続けて作りましょうか？




了解しました。RunWithLoadingAsync と同じ動きをする同期版の RunWithLoading を用意します。


---

目的

非同期 (async Task) ではなく、通常の void や Action を使った同期処理向け

WinForms の UI スレッドがブロックされないように、LoadingForm を別スレッドで表示

CancellationToken によるキャンセルサポートあり



---

実装：RunWithLoading（マーキー版、同期処理）

public static void RunWithLoading(string message, Action<CancellationToken> action)
{
    var loadingFormReady = new ManualResetEvent(false);
    CancellationTokenSource cts = new CancellationTokenSource();
    LoadingForm loadingForm = null;

    var uiThread = new Thread(() =>
    {
        loadingForm = new LoadingForm(message, useProgress: false);
        loadingForm.FormClosed += (_, __) => Application.ExitThread(); // 必須
        loadingFormReady.Set();
        Application.Run(loadingForm);
    });

    uiThread.SetApartmentState(ApartmentState.STA);
    uiThread.Start();

    // LoadingFormの初期化完了を待つ
    loadingFormReady.WaitOne();

    try
    {
        action(loadingForm.Token); // 同期処理の実行
    }
    catch (OperationCanceledException)
    {
        // キャンセル時の例外をここで握りつぶす or 投げ直す
    }
    finally
    {
        // 終了後にフォームを閉じる
        loadingForm?.Invoke(() => loadingForm.Close());
        uiThread.Join();
    }
}


---

使用例（同期マーキー処理）

RunWithLoading("同期処理中...", token =>
{
    for (int i = 0; i < 5; i++)
    {
        token.ThrowIfCancellationRequested();
        Thread.Sleep(500); // 疑似処理
    }
});


---

補足と拡張案

RunWithProgressLoading の同期版も必要であれば同様に作成可能です（IProgress<int> 使用）

try-catch 内で例外を UI 上に表示する仕組みも追加可能です（例：ShowStatusMessage() ）



---

同期版のプログレス表示付き（％あり）バージョンも作りましょうか？




AppContext に EventBus を組み込むことで、WinForms アプリケーション全体で疎結合なイベント通知を実現できます。以下に、リアルな構成を示します。


---

1. EventBusの目的

コンポーネント間の通知（例：ProjectLoaded を全体に通知）

状態の変更（例：ThemeChanged → UI 全体更新）

非同期タスクの完了通知（例：ファイル読込完了）



---

2. AppContextにEventBusを持たせる構成例

public class AppContext
{
    public AppSettings Settings { get; }
    public EventBus EventBus { get; }
    public ProjectContext? CurrentProject { get; set; }

    public AppContext(AppSettings settings)
    {
        Settings = settings;
        EventBus = new EventBus();
    }
}


---

3. EventBus 実装例

public class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        if (!_handlers.ContainsKey(typeof(TEvent)))
            _handlers[typeof(TEvent)] = new List<Delegate>();

        _handlers[typeof(TEvent)].Add(handler);
    }

    public void Publish<TEvent>(TEvent evt)
    {
        if (_handlers.TryGetValue(typeof(TEvent), out var delegates))
        {
            foreach (var handler in delegates.OfType<Action<TEvent>>())
            {
                handler(evt);
            }
        }
    }

    public void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        if (_handlers.TryGetValue(typeof(TEvent), out var delegates))
        {
            delegates.Remove(handler);
        }
    }
}


---

4. イベントクラス定義の例

public class ProjectLoadedEvent
{
    public ProjectContext Project { get; }

    public ProjectLoadedEvent(ProjectContext project)
    {
        Project = project;
    }
}

public class ThemeChangedEvent
{
    public string NewTheme { get; }

    public ThemeChangedEvent(string newTheme)
    {
        NewTheme = newTheme;
    }
}


---

5. 使用例

購読（フォームやコンポーネントで）

_appContext.EventBus.Subscribe<ProjectLoadedEvent>(e =>
{
    MessageBox.Show($"プロジェクト {e.Project.Name} が読み込まれました");
});

発行（プロジェクト読み込み後など）

_appContext.EventBus.Publish(new ProjectLoadedEvent(loadedProject));


---

拡張案

async Task 対応で非同期イベントをサポート（例：Func<TEvent, Task>）

イベントログを記録してデバッグに利用

グローバルロギングと連携

優先度・フィルタ・トピックベースの配信対応（複雑化するなら）



---

この方式はユーザーの WinForms + DIなし構成 に自然に組み込めるシンプル設計です。他のパーツ（例：NavigationService, MessageService）との連携案もご紹介できます。必要であればどうぞ！




WinFormsアプリケーションで、AppSetting, AppContext, ProjectContext を分けて扱う場合、各コンテキストが管理するべきリアルワールドな要素（保存・読込対象）は以下のように設計できます。


---

1. AppSetting（アプリケーション設定）

アプリケーション全体の永続的な設定（マシン/ユーザー単位）。基本的にアプリの実行ファイルに紐づく。

保存対象例：

ウィンドウの初期サイズ・位置

テーマ（ライト/ダーク）

使用言語（ja/en など）

最後に開いたプロジェクトのパス

APIキーや外部サービスの接続情報（暗号化推奨）

自動保存やバックアップの間隔設定

デフォルトの保存フォルダパス

アップデート確認の有無

使用するプラグイン一覧や状態


保存場所例：

Properties.Settings

JSONファイル（appsettings.json）

レジストリ（古典的だが、近年は非推奨）



---

2. AppContext（アプリ実行中の一時状態）

アプリケーション実行中に使う共有状態で、永続化されない。

保存対象例：

現在ログイン中のユーザー情報

現在編集中のドキュメント

現在選択中のプロジェクトやファイル

ローディング中かどうかの状態フラグ

サービスのインスタンスやイベントバス

タスクマネージャーやスケジューラの状態

ナビゲーションの現在の画面情報


> 主に DI（ServiceProvider）や Singleton でアプリ起動中に保持。




---

3. ProjectContext（プロジェクト単位の情報）

ユーザーが開いているプロジェクト固有の設定やデータを扱う。ユーザーが保存操作を行う対象。

保存対象例：

プロジェクトの名前・説明

プロジェクト内のファイル一覧（構成）

チャート設定、レイアウト構成

プロジェクトのパスや相対ディレクトリ

波形シーケンス、デバイス設定、シナリオ情報（ユーザーの例に合わせ）

プロジェクト固有の変数定義、マッピング

タイムラインや履歴、バージョン情報


保存場所例：

project.json や独自の形式ファイル（XMLなど）

複数ファイルをまとめたフォルダ構成（例: *.proj/ ディレクトリ）



---

補足：構成イメージ（クラス階層）

public class AppSettings {
    public string Language { get; set; }
    public string Theme { get; set; }
    public string LastProjectPath { get; set; }
    // ...
}

public class AppContext {
    public AppSettings Settings { get; }
    public UserInfo CurrentUser { get; set; }
    public ITaskManager TaskManager { get; }
    public NavigationService Navigation { get; }
    // ...
}

public class ProjectContext {
    public string Name { get; set; }
    public string BasePath { get; set; }
    public List<ProjectItem> Items { get; set; }
    public List<WaveSequence> WaveSequences { get; set; }
    public DeviceSetting DeviceSetting { get; set; }
    // ...
}


---

拡張案

AppSettingにバージョン管理を導入し、旧フォーマットとの互換性チェック。

ProjectContextにUndo/Redo履歴やタイムスタンプ履歴を持たせる。

AppContextにローカルメッセージバス（EventBus）やログサービスを組み込んで、デバッグやUI表示に利用。

ProjectContextをZip形式で保存し、リソースを同梱する（画像、設定などをまとめる）。



---

必要に応じて、保存・読み込みの実装例も提示できます。どの部分を深掘りましょうか？




IValidator を INotifyPropertyChanged と組み合わせると、「プロパティが変更されたときに自動でバリデーション → エラー表示」といったMVVM的なUI連携が可能になります。以下はその基本実装です。


---

1. BindableValidatorViewModel<T>：INotifyPropertyChanged + IValidator 対応

public class BindableValidatorViewModel<T> : INotifyPropertyChanged
{
    private T _value;
    private string _errorMessage;
    private readonly IValidator<T> _validator;

    public BindableValidatorViewModel(IValidator<T> validator)
    {
        _validator = validator;
    }

    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
                Validate(); // 値が変わるたびにバリデーション
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    private void Validate()
    {
        if (!_validator.Validate(_value, out var message))
        {
            ErrorMessage = message;
        }
        else
        {
            ErrorMessage = string.Empty;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}


---

2. 使用例：WinFormsやWPFバインディング向け

// バリデータを組み合わせる
var validator = new MultipleValidator<int>(new IValidator<int>[] {
    new RangeValidator<int>(1, 100),
    new NotEqualValidator<int>(0)
});

var viewModel = new BindableValidatorViewModel<int>(validator);

// バインディング対象：ViewModel.Value, ViewModel.ErrorMessage
viewModel.Value = 150;
Console.WriteLine(viewModel.HasError); // true
Console.WriteLine(viewModel.ErrorMessage); // "値は 1 以上 100 以下でなければなりません。"


---

3. 拡張案

BindableValidatorViewModel をジェネリックから BindableStringValidator, BindableIntValidator など具体型に分けて使いやすく

IDataErrorInfo や INotifyDataErrorInfo に対応（WPF用）

WinFormsの ErrorProvider と連携して即時フィードバック（例：テキストボックスにエラーマーク）



---

4. WinForms用ErrorProvider連携例

errorProvider1.SetError(textBox1, viewModel.ErrorMessage);


---

もっとUI側に最適化したバリデーションコンポーネント化（UserControl + ViewModel）も可能です。必要であれば設計案出します。どうしますか？




RangeValidator と MultipleValidator を IValidator<T> を使って実装する例を以下に示します。これはWinFormsの入力検証などに便利です。


---

1. RangeValidator<T>：範囲チェック用バリデータ

public class RangeValidator<T> : IValidator<T> where T : IComparable<T>
{
    private readonly T _min;
    private readonly T _max;

    public RangeValidator(T min, T max)
    {
        _min = min;
        _max = max;
    }

    public bool Validate(T input, out string errorMessage)
    {
        if (input.CompareTo(_min) < 0 || input.CompareTo(_max) > 0)
        {
            errorMessage = $"値は {_min} 以上 {_max} 以下でなければなりません。";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}


---

2. MultipleValidator<T>：複数バリデータの合成

public class MultipleValidator<T> : IValidator<T>
{
    private readonly IEnumerable<IValidator<T>> _validators;

    public MultipleValidator(IEnumerable<IValidator<T>> validators)
    {
        _validators = validators;
    }

    public bool Validate(T input, out string errorMessage)
    {
        foreach (var validator in _validators)
        {
            if (!validator.Validate(input, out errorMessage))
            {
                return false;
            }
        }

        errorMessage = string.Empty;
        return true;
    }
}


---

3. 使用例（WinForms などで）

var rangeValidator = new RangeValidator<int>(1, 100);
var notZeroValidator = new NotEqualValidator<int>(0); // 自作バリデータと想定

var combinedValidator = new MultipleValidator<int>(new IValidator<int>[] {
    rangeValidator,
    notZeroValidator
});

if (!combinedValidator.Validate(int.Parse(textBox1.Text), out string error))
{
    MessageBox.Show(error, "検証エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}


---

拡張案

OptionalValidator<T>：nullや空文字はスキップ

RegexValidator：正規表現による文字列チェック

IValidator<object> ベースの汎用対応

エラー詳細を ValidationResult クラスで返す

UIコンポーネント（TextBoxなど）に直接バインド可能なバリデータフレームワークの構築



---

さらにUI連携やバリデーションルールの管理なども含めたアーキテクチャを作りたい場合は、その構成例も出せます。必要ですか？





了解です！ObservableCollection<T> に対して Undo / Redo をサポートするように拡張します。


---

方針

ICommand パターンに基づく操作履歴を保持

UndoStack / RedoStack を使って操作の巻き戻しとやり直しを実装



---

拡張版 CollectionClipboard<T>

public class CollectionClipboard<T>
{
    private readonly ObservableCollection<T> _collection;
    private List<T> _clipboard = new();

    private readonly Stack<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();

    public CollectionClipboard(ObservableCollection<T> collection)
    {
        _collection = collection;
    }

    // -------- コピー＆カット --------
    public void Copy(IEnumerable<T> items)
    {
        _clipboard = items.Select(Clone).ToList();
    }

    public void CopyAt(int index, int count)
    {
        var items = _collection.Skip(index).Take(count).ToList();
        Copy(items);
    }

    public void Cut(IEnumerable<T> items)
    {
        Copy(items);
        var command = new RemoveCommand<T>(_collection, items.ToList());
        command.Execute();
        PushUndo(command);
    }

    public void CutAt(int index, int count)
    {
        var items = _collection.Skip(index).Take(count).ToList();
        Cut(items);
    }

    // -------- ペースト --------
    public void Paste(int index)
    {
        var items = _clipboard.Select(Clone).ToList();
        var command = new InsertCommand<T>(_collection, items, index);
        command.Execute();
        PushUndo(command);
    }

    // -------- 挿入・削除 --------
    public void Insert(int index, T item)
    {
        var command = new InsertCommand<T>(_collection, new List<T> { item }, index);
        command.Execute();
        PushUndo(command);
    }

    public void Delete(IEnumerable<T> items)
    {
        var command = new RemoveCommand<T>(_collection, items.ToList());
        command.Execute();
        PushUndo(command);
    }

    // -------- Undo / Redo --------
    public void Undo()
    {
        if (_undoStack.Any())
        {
            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
        }
    }

    public void Redo()
    {
        if (_redoStack.Any())
        {
            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }
    }

    private void PushUndo(ICommand command)
    {
        _undoStack.Push(command);
        _redoStack.Clear();
    }

    private T Clone(T item) => item;
}


---

コマンドインターフェースと実装

public interface ICommand
{
    void Execute();
    void Undo();
}

public class InsertCommand<T> : ICommand
{
    private readonly ObservableCollection<T> _collection;
    private readonly List<T> _items;
    private readonly int _index;

    public InsertCommand(ObservableCollection<T> collection, List<T> items, int index)
    {
        _collection = collection;
        _items = items;
        _index = index;
    }

    public void Execute()
    {
        for (int i = 0; i < _items.Count; i++)
            _collection.Insert(_index + i, _items[i]);
    }

    public void Undo()
    {
        for (int i = 0; i < _items.Count; i++)
            _collection.RemoveAt(_index);
    }
}

public class RemoveCommand<T> : ICommand
{
    private readonly ObservableCollection<T> _collection;
    private readonly List<T> _items;
    private readonly List<int> _indices = new();

    public RemoveCommand(ObservableCollection<T> collection, List<T> items)
    {
        _collection = collection;
        _items = items;
    }

    public void Execute()
    {
        foreach (var item in _items)
        {
            int index = _collection.IndexOf(item);
            if (index >= 0)
            {
                _indices.Add(index);
                _collection.RemoveAt(index);
            }
        }
    }

    public void Undo()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            _collection.Insert(_indices[i], _items[i]);
        }
    }
}


---

使用例

var list = new ObservableCollection<string> { "A", "B", "C" };
var manager = new CollectionClipboard<string>(list);

manager.CutAt(0, 1); // "A" をカット → list: B, C
manager.Undo();      // "A" を戻す → list: A, B, C
manager.Redo();      // 再カット → list: B, C


---

拡張案

Undo/Redo 上限数の制限

変更イベントとの統合（UI更新連動）

コマンド履歴のラベル管理（例: "カット A" → UI表示）



---

WinFormsでこのUndo/Redoをボタンやショートカットと連携する例も出せます。必要ならお知らせください！






public class PageModelFactory
{
    public static List<PageModel<TItem>> CreatePagesFromContexts<TItem>(
        List<TItem> allItems,
        List<IPageContext> contexts)
    {
        var result = new List<PageModel<TItem>>();
        int currentIndex = 0;

        foreach (var context in contexts)
        {
            int itemCount = GetItemCountFromContext(context);

            var pageItems = allItems
                .Skip(currentIndex)
                .Take(itemCount)
                .ToList();

            result.Add(new PageModel<TItem>(pageItems, context));

            currentIndex += itemCount;
        }

        return result;
    }

    private static int GetItemCountFromContext(IPageContext context)
    {
        return context switch
        {
            ListContext lc => lc.ItemCount,
            GridContext gc => gc.Rows * gc.Columns,
            _ => throw new InvalidOperationException("Unknown context type"),
        };
    }
}


了解です！角丸モードを追加するには、カスタムパネルに「通常の矩形枠」と「角丸枠」を切り替えられるプロパティを用意して、描画処理を分岐させればOKです。


---

角丸枠対応のカスタムパネル例（C#）

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class BorderedPanel : Panel
{
    public Color BorderColor { get; set; } = Color.Black;
    public int BorderWidth { get; set; } = 2;
    public bool RoundedCorners { get; set; } = false;
    public int CornerRadius { get; set; } = 10;

    public BorderedPanel()
    {
        this.DoubleBuffered = true;
        this.ResizeRedraw = true;
        this.BackColor = Color.White;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        using (Pen pen = new Pen(BorderColor, BorderWidth))
        {
            pen.Alignment = PenAlignment.Inset;

            Rectangle rect = new Rectangle(
                0,
                0,
                this.ClientSize.Width - 1,
                this.ClientSize.Height - 1
            );

            if (RoundedCorners)
            {
                using (GraphicsPath path = GetRoundedRectPath(rect, CornerRadius))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(pen, path);
                }
            }
            else
            {
                e.Graphics.DrawRectangle(pen, rect);
            }
        }
    }

    private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();

        int diameter = radius * 2;
        Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

        // 左上
        path.AddArc(arc, 180, 90);

        // 右上
        arc.X = rect.Right - diameter;
        path.AddArc(arc, 270, 90);

        // 右下
        arc.Y = rect.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        // 左下
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }
}


---

使い方

var panel = new BorderedPanel
{
    BorderColor = Color.Blue,
    BorderWidth = 3,
    RoundedCorners = true,
    CornerRadius = 12,
    Size = new Size(200, 100),
    Location = new Point(10, 10)
};


---

拡張案

RoundedCornersMode のように「上だけ角丸」なども実装可能。

ホバー時に色が変わるなどのインタラクション効果。

枠だけでなく背景もグラデーションやパターン描画。


さらに拡張したい方向性があれば、それに合わせた提案もできます！




了解です！
それでは、定期実行の動作制御を柔軟に切り替えるために、OverrunStrategy を導入した形で ScheduledTaskBase を拡張します。


---

✅ OverrunStrategy とは？

定期実行タスクで、処理がスケジュール時間を過ぎて終了した場合の挙動を切り替える戦略です。


---

1. 🔧 列挙型 OverrunStrategy

public enum OverrunStrategy
{
    FixedInterval,    // 固定間隔（前回終了から一定間隔）
    CatchUp,          // スケジュール基準で即時実行
    Skip              // スケジュール基準、次の時刻までスキップ
}


---

2. 🔧 ScheduledTaskBase に統合

public abstract class ScheduledTaskBase : TaskBase
{
    public TimeSpan Interval { get; }
    public OverrunStrategy OverrunStrategy { get; }

    protected ScheduledTaskBase(string name, TimeSpan interval, OverrunStrategy overrunStrategy = OverrunStrategy.FixedInterval)
        : base(name)
    {
        Interval = interval;
        OverrunStrategy = overrunStrategy;
    }

    protected abstract Task ExecuteOnceAsync();

    protected override async Task ExecuteAsync()
    {
        var nextRunTime = DateTime.Now;

        while (!CancellationToken.IsCancellationRequested)
        {
            var now = DateTime.Now;

            if (OverrunStrategy == OverrunStrategy.Skip && now < nextRunTime)
            {
                await Task.Delay(nextRunTime - now, CancellationToken);
            }

            var actualStart = DateTime.Now;
            await ExecuteOnceAsync();
            var actualEnd = DateTime.Now;

            switch (OverrunStrategy)
            {
                case OverrunStrategy.FixedInterval:
                    var waitTime = Interval - (actualEnd - actualStart);
                    if (waitTime > TimeSpan.Zero)
                        await Task.Delay(waitTime, CancellationToken);
                    break;

                case OverrunStrategy.CatchUp:
                    nextRunTime += Interval;
                    break;

                case OverrunStrategy.Skip:
                    nextRunTime += Interval;
                    if (nextRunTime < DateTime.Now)
                    {
                        // スキップ（遅れているので次の1回分飛ばす）
                        nextRunTime = DateTime.Now + Interval;
                        ReportProgress("スケジュール遅延によりスキップ");
                    }
                    break;
            }
        }
    }
}


---

3. ✅ 使用例

var heartbeat = new HeartbeatTask(
    name: "Heartbeat",
    interval: TimeSpan.FromSeconds(5),
    overrunStrategy: OverrunStrategy.Skip
);

timerService.StartTask(heartbeat);


---

✅ 各戦略の比較


---

🧩 拡張案


---

OverrunStrategy により柔軟な制御ができるようになりました。
必要なら ログ出力付きテンプレート や 構成ファイルから自動生成 も追加できます。

もっと進めますか？




了解です！
以下は UIバインディングに対応 しつつ、Save/Load メソッドを持たずに JSON に保存できる形 にした MruList<T> の実装です。

特徴

ObservableCollection<T> に基づいた実装で WPF や WinForms のバインディングにそのまま使えます。

System.Text.Json を使って直列化・保存・読み込み可能。

保存やロードは外部から JsonSerializer を用いて行う想定。



---

実装例

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

public class MruList<T> : ObservableCollection<T>
{
    private int _capacity;

    public MruList(int capacity = 10)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        _capacity = capacity;
    }

    [JsonInclude]
    public int Capacity
    {
        get => _capacity;
        private set => _capacity = value;
    }

    public void Use(T item)
    {
        if (this.Contains(item))
        {
            this.Remove(item);
        }
        this.Insert(0, item);

        while (this.Count > Capacity)
        {
            this.RemoveAt(this.Count - 1);
        }
    }

    public bool TryGetRecent(out T? item)
    {
        if (this.Count > 0)
        {
            item = this[0];
            return true;
        }
        item = default;
        return false;
    }
}


---

使用例：保存・読み込み（WPF/WinForms）

using System.Text.Json;

// 保存
var recent = new MruList<string>(5);
recent.Use("file1.txt");
recent.Use("file2.txt");

string json = JsonSerializer.Serialize(recent);

// 読み込み
var loaded = JsonSerializer.Deserialize<MruList<string>>(json);


---

拡張案

1. INotifyPropertyChanged のサポート（SelectedItemなどを追加してUIとの双方向連携強化）


2. 重複判定に IEqualityComparer<T> をサポート


3. 項目にタイムスタンプを含めるオプション


4. バインディング専用ラッパーの作成（BindingListAdapter<T>のような形で）



これを使いたいUI（WPF、WinForms、Blazorなど）に合わせた具体例も出せますので、必要であれば教えてください。




WinForms の ComboBox の オートコンプリート 機能と連携させるために、さきほどの MruList<T> をベースにした 文字列用のMRUクラス を ComboBox にバインドする方法を紹介します。


---

【1】MruList<string> クラス（前のままでOK）

（前述の MruList<T> を string で使うだけ）


---

【2】WinForms ComboBox と連携するコード例

public partial class MainForm : Form
{
    private MruList<string> _recentItems;

    public MainForm()
    {
        InitializeComponent();
        InitializeComboBox();
    }

    private void InitializeComboBox()
    {
        _recentItems = new MruList<string>(10);

        comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        comboBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;
        comboBox1.DataSource = _recentItems;
    }

    private void buttonUse_Click(object sender, EventArgs e)
    {
        var text = comboBox1.Text?.Trim();
        if (!string.IsNullOrEmpty(text))
        {
            _recentItems.Use(text);
            UpdateAutoCompleteSource();
        }
    }

    private void UpdateAutoCompleteSource()
    {
        var ac = new AutoCompleteStringCollection();
        ac.AddRange(_recentItems.ToArray());
        comboBox1.AutoCompleteCustomSource = ac;
    }
}


---

【ポイント】

ComboBox.DataSource に MruList<string> を直接設定し、表示と選択をバインド。

AutoCompleteCustomSource は AutoCompleteStringCollection を使って更新。

Use() で履歴が更新されたら AutoCompleteCustomSource も更新。



---

拡張案

1. 履歴を JSON に保存：終了時に JsonSerializer.Serialize(_recentItems) で保存し、起動時に Deserialize して復元。


2. 入力確定イベントで自動登録：Enter キーで履歴追加。


3. 最近使用した順に並べてドロップダウン表示：DataSource がそれに対応済み。




---

他にも、UI に履歴削除ボタンや上限数の変更機能などを付けたい場合も対応できます。必要であれば教えてください。





できます！Windowsの「ファイルのプロパティ > 詳細タブ」に表示されるような、MP3のメタデータ（再生時間、ビットレート、アーティスト名など）は、Shell32.dll を使うことでライブラリなしでも取得可能です。


---

方法：Shell32 を使ってファイルの詳細情報を取得する

1. 参照設定

まず、プロジェクトに以下のCOM参照を追加してください：

Shell32（Microsoft Shell Controls And Automation）


または、using Shell32; を使うために COM 参照を手動で追加する必要があります（Visual Studio の場合：プロジェクト → 参照の追加 → COM → Shell32）。


---

2. サンプルコード（再生時間などの取得）

using System;
using Shell32;

class Mp3PropertyReader
{
    public static void PrintMp3Properties(string filePath)
    {
        Shell shell = new Shell();
        string folderPath = System.IO.Path.GetDirectoryName(filePath);
        string fileName = System.IO.Path.GetFileName(filePath);
        Folder folder = shell.NameSpace(folderPath);
        FolderItem item = folder.ParseName(fileName);

        Console.WriteLine($"ファイル: {fileName}");
        for (int i = 0; i < 300; i++)
        {
            string key = folder.GetDetailsOf(null, i);
            string value = folder.GetDetailsOf(item, i);

            if (!string.IsNullOrEmpty(value))
                Console.WriteLine($"{i}: {key} = {value}");
        }
    }
}


---

出力例（一部抜粋）

0: 名前 = sample.mp3
1: サイズ = 4.56 MB
...
27: 再生時間 = 00:03:15
28: ビットレート = 128kbps
...
33: アーティスト = Sample Artist
...


---

注意点

インデックス番号（例：27 = 再生時間）は環境やWindowsのバージョンによって異なる可能性があります。forループで一覧出力して確認するのがおすすめです。

この方法は Windows に依存します（.NET Core/6+ では Windows 限定で動作）。

ファイルが壊れていたり、プロパティが存在しない場合は空になることがあります。



---

必要であれば、特定のプロパティだけ抽出するコードにしてもいいですよ。どうします？



了解です！
以下は ネストされたプロパティに加えて、コレクションや配列にも対応した C# のプロパティ列挙コードです。


---

対応内容:

プリミティブ型や文字列は直接表示

ネストされたクラスは再帰処理

IEnumerable（List, Arrayなど）はループして要素も再帰処理



---

コード

using System;
using System.Collections;
using System.Reflection;

public class PropertyLister
{
    public static void ListProperties(object obj, string prefix = "")
    {
        if (obj == null) return;

        Type type = obj.GetType();

        // IEnumerable だが string は除外
        if (obj is IEnumerable enumerable && type != typeof(string))
        {
            int index = 0;
            foreach (var item in enumerable)
            {
                string itemPrefix = $"{prefix}[{index}]";
                ListProperties(item, itemPrefix);
                index++;
            }
            return;
        }

        // プリミティブ型 or string の場合
        if (type.IsPrimitive || obj is string || obj is decimal)
        {
            Console.WriteLine($"{prefix} = {obj}");
            return;
        }

        // 通常のクラスのプロパティ処理
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            object value = prop.GetValue(obj);
            string propName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
            ListProperties(value, propName);
        }
    }
}


---

テストクラス

public class Address
{
    public string City { get; set; }
    public string Zip { get; set; }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Address HomeAddress { get; set; }
    public string[] Tags { get; set; }
}

public class Group
{
    public string GroupName { get; set; }
    public List<Person> Members { get; set; }
}


---

使用例

var group = new Group
{
    GroupName = "Developers",
    Members = new List<Person>
    {
        new Person
        {
            Name = "Alice",
            Age = 25,
            HomeAddress = new Address { City = "Tokyo", Zip = "100-0001" },
            Tags = new[] { "Frontend", "Vue" }
        },
        new Person
        {
            Name = "Bob",
            Age = 30,
            HomeAddress = new Address { City = "Osaka", Zip = "530-0001" },
            Tags = new[] { "Backend", "Rust" }
        }
    }
};

PropertyLister.ListProperties(group);


---

出力例

GroupName = Developers
Members[0].Name = Alice
Members[0].Age = 25
Members[0].HomeAddress.City = Tokyo
Members[0].HomeAddress.Zip = 100-0001
Members[0].Tags[0] = Frontend
Members[0].Tags[1] = Vue
Members[1].Name = Bob
Members[1].Age = 30
Members[1].HomeAddress.City = Osaka
Members[1].HomeAddress.Zip = 530-0001
Members[1].Tags[0] = Backend
Members[1].Tags[1] = Rust


---

拡張案

1. 循環参照対策（一度出たオブジェクトはスキップ）


2. 最大深さ指定（深すぎるネストを制限）


3. プロパティフィルタ（特定の属性がついたものだけ表示）


4. JSON 形式出力対応


5. 非公開プロパティやフィールドへの対応



必要な機能があればどんどん追加できます。どうしましょうか？




HttpClient のレスポンスを共通クラスとして扱うために、ステータスコード（int）、レスポンスボディ（string）、およびエンドポイントごとの JSON を任意のクラス型として扱えるようにジェネリッククラスを定義するのが効果的です。

以下に C# の実装例を示します。


---

共通レスポンスクラスの定義

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string RawBody { get; set; }
    public T? Body { get; set; }
}


---

使用例：HttpClientの呼び出しメソッド

public static async Task<ApiResponse<T>> SendRequestAsync<T>(HttpClient client, HttpRequestMessage request)
{
    var response = await client.SendAsync(request);
    var rawBody = await response.Content.ReadAsStringAsync();

    T? body = default;

    try
    {
        body = JsonSerializer.Deserialize<T>(rawBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    catch (JsonException)
    {
        // ログやエラーハンドリングが必要ならここに
    }

    return new ApiResponse<T>
    {
        StatusCode = (int)response.StatusCode,
        RawBody = rawBody,
        Body = body
    };
}


---

利用例：任意のエンドポイント用DTO

public class UserResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

// 呼び出し例
var client = new HttpClient();
var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/users/1");

var response = await SendRequestAsync<UserResponse>(client, request);

Console.WriteLine($"Status: {response.StatusCode}");
Console.WriteLine($"User Name: {response.Body?.Name}");


---

拡張案：

エラーメッセージやヘッダーを含める

public Dictionary<string, IEnumerable<string>> Headers { get; set; }
public string? ErrorMessage { get; set; }

成功／失敗を簡単に判定

public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

ログ出力やAPI呼び出し履歴を記録する機能との連携


他にも欲しい構造や例があれば教えてください、それに合わせて拡張します！




了解です！**「1. 文字列パース機能」と「4. IComparable 実装」**を追加した SerialNumber クラスの完全版を以下に示します。


---

改良版 SerialNumber クラス（C#）

using System;

public class SerialNumber : IEquatable<SerialNumber>, ICloneable, IComparable<SerialNumber>
{
    public string Prefix { get; set; }
    public string Suffix { get; set; }
    public int Number { get; set; }

    public SerialNumber(string prefix, int number, string suffix)
    {
        if (number < 0 || number > 999999)
            throw new ArgumentOutOfRangeException(nameof(number), "Number must be between 0 and 999999.");

        Prefix = prefix ?? string.Empty;
        Suffix = suffix ?? string.Empty;
        Number = number;
    }

    public override string ToString()
    {
        return $"{Prefix}{Number:D6}{Suffix}";
    }

    public static SerialNumber Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 6)
            throw new FormatException("Invalid serial number format.");

        // 数字部分を抽出（最初の6桁連続数字を探す）
        int start = -1;
        for (int i = 0; i <= input.Length - 6; i++)
        {
            if (int.TryParse(input.Substring(i, 6), out _))
            {
                start = i;
                break;
            }
        }

        if (start == -1)
            throw new FormatException("No 6-digit number found in the input.");

        string prefix = input.Substring(0, start);
        string numberStr = input.Substring(start, 6);
        string suffix = input.Substring(start + 6);

        return new SerialNumber(prefix, int.Parse(numberStr), suffix);
    }

    public bool Equals(SerialNumber? other)
    {
        if (other == null) return false;
        return Prefix == other.Prefix && Number == other.Number && Suffix == other.Suffix;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SerialNumber);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Prefix, Number, Suffix);
    }

    public object Clone()
    {
        return new SerialNumber(Prefix, Number, Suffix);
    }

    public int CompareTo(SerialNumber? other)
    {
        if (other == null) return 1;

        int prefixComp = string.Compare(Prefix, other.Prefix, StringComparison.Ordinal);
        if (prefixComp != 0) return prefixComp;

        int numberComp = Number.CompareTo(other.Number);
        if (numberComp != 0) return numberComp;

        return string.Compare(Suffix, other.Suffix, StringComparison.Ordinal);
    }
}


---

使用例

var s1 = new SerialNumber("A", 123456, "X");
var s2 = SerialNumber.Parse("A123456X");
var s3 = new SerialNumber("A", 123457, "X");

Console.WriteLine(s1.Equals(s2)); // True
Console.WriteLine(s1.CompareTo(s3)); // -1（s1のほうが小さい）

var list = new List<SerialNumber> { s3, s1, s2 };
list.Sort();

foreach (var sn in list)
    Console.WriteLine(sn); // A123456X, A123456X, A123457X


---

拡張案（さらなる発展）

TryParse メソッドで安全にパース

SerialNumberFormat を作ってフォーマットカスタマイズ対応

JSON や XML とのシリアライズ対応


他にも拡張したい項目があればどうぞ！





いいですね！
以下の3つのメソッドを含むユーティリティクラス SerialNumberListExtensions を実装しました。


---

■ 機能一覧


---

■ 実装コード（C#）

using System;
using System.Collections.Generic;
using System.Linq;

public static class SerialNumberListExtensions
{
    public static bool IsSequentialSamePrefixAndSuffix(this IEnumerable<SerialNumber> list)
    {
        var sorted = list.OrderBy(sn => sn.Number).ToList();

        if (!sorted.Any()) return false;

        var first = sorted.First();
        return sorted.All(sn => sn.Prefix == first.Prefix && sn.Suffix == first.Suffix) &&
               sorted.Zip(sorted.Skip(1), (a, b) => b.Number - a.Number).All(diff => diff == 1);
    }

    public static Dictionary<string, List<SerialNumber>> GroupBySuffix(this IEnumerable<SerialNumber> list)
    {
        return list.GroupBy(sn => sn.Suffix)
                   .ToDictionary(g => g.Key, g => g.ToList());
    }

    public static string ToSerialString(this IEnumerable<SerialNumber> list)
    {
        var sorted = list.OrderBy(sn => sn.Number).ToList();

        if (!sorted.Any()) return string.Empty;

        List<string> result = new();
        int start = 0;

        while (start < sorted.Count)
        {
            int end = start;
            while (end + 1 < sorted.Count &&
                   sorted[end + 1].Number == sorted[end].Number + 1 &&
                   sorted[end + 1].Prefix == sorted[start].Prefix &&
                   sorted[end + 1].Suffix == sorted[start].Suffix)
            {
                end++;
            }

            if (end > start)
            {
                result.Add($"{sorted[start]}〜{sorted[end]}");
                start = end + 1;
            }
            else
            {
                result.Add(sorted[start].ToString());
                start++;
            }
        }

        return string.Join(", ", result);
    }
}


---

■ 使用例

var list = new List<SerialNumber>
{
    new("SN", 1, "A"),
    new("SN", 2, "A"),
    new("SN", 3, "A"),
    new("SN", 5, "A"),
    new("SN", 6, "A"),
    new("SN", 10, "B"),
};

Console.WriteLine(list.IsSequentialSamePrefixAndSuffix()); // False

var groups = list.GroupBySuffix();
foreach (var kv in groups)
{
    Console.WriteLine($"Suffix: {kv.Key}");
    foreach (var sn in kv.Value)
        Console.WriteLine("  " + sn);
}

Console.WriteLine(list.ToSerialString());
// SN000001A〜SN000003A, SN000005A〜SN000006A, SN000010B


---

■ 拡張案

ToSerialStringGroupedBySuffix()：Suffixごとにグループ化して、それぞれ ToSerialString 表記で出力

ToJson() / ToCsv() 対応の整形出力

別スレッドでパラレルに判定（大規模データ用）



---

必要なら上記の拡張もすぐ作れます！どれか作ってみましょうか？




了解です！以下のように仕様を整理し、クラス設計を改善・拡張します。


---

改訂仕様

● 全体仕様

入力リストを、設定された数に 明示的に分割（各セグメントのアイテム数を指定）

各セグメントに対して以下を個別に設定：

Count: そのセグメントに含めるアイテム数（＝強制的にその数に分ける）

SortOrder: 昇順 or 降順

Meta: 任意のメタ情報（ラベルなど）




---

設計

PartitionSettings の拡張

public enum SortOrder
{
    Ascending,
    Descending
}

public class PartitionSettings
{
    public int Count { get; set; }  // 分割数（固定）
    public SortOrder SortOrder { get; set; }
    public string Meta { get; set; } // 任意のラベルなど
}


---

PartitionResult（結果＋メタ情報）

public class PartitionResult<T>
{
    public string Meta { get; set; }
    public List<T> Items { get; set; }
}


---

ListPartitioner 実装（強制分割＋メタ付き）

public class ListPartitioner<T> where T : IComparable<T>
{
    private readonly List<T> _source;
    private readonly List<PartitionSettings> _settings;

    public ListPartitioner(List<T> source, List<PartitionSettings> settings)
    {
        if (settings == null || settings.Count == 0)
            throw new ArgumentException("Partition settings must not be null or empty.");

        _source = source ?? throw new ArgumentNullException(nameof(source));
        _settings = settings;

        var totalCount = settings.Sum(s => s.Count);
        if (totalCount > _source.Count)
            throw new ArgumentException("Total partition counts exceed source list count.");
    }

    public List<PartitionResult<T>> Partition()
    {
        var result = new List<PartitionResult<T>>();
        int currentIndex = 0;

        foreach (var setting in _settings)
        {
            var segment = _source.Skip(currentIndex).Take(setting.Count).ToList();
            currentIndex += setting.Count;

            if (setting.SortOrder == SortOrder.Ascending)
                segment.Sort();
            else
                segment.Sort((a, b) => b.CompareTo(a));

            result.Add(new PartitionResult<T>
            {
                Meta = setting.Meta,
                Items = segment
            });
        }

        return result;
    }
}


---

使用例

var source = Enumerable.Range(1, 15).ToList();

var settings = new List<PartitionSettings>
{
    new PartitionSettings { Count = 5, SortOrder = SortOrder.Descending, Meta = "Group A" },
    new PartitionSettings { Count = 5, SortOrder = SortOrder.Ascending, Meta = "Group B" },
    new PartitionSettings { Count = 5, SortOrder = SortOrder.Descending, Meta = "Group C" },
};

var partitioner = new ListPartitioner<int>(source, settings);
var partitions = partitioner.Partition();

foreach (var p in partitions)
{
    Console.WriteLine($"{p.Meta}: {string.Join(", ", p.Items)}");
}


---

出力例

Group A: 5, 4, 3, 2, 1
Group B: 6, 7, 8, 9, 10
Group C: 15, 14, 13, 12, 11


---

拡張案

1. Meta を Dictionary<string, object> にして汎用的なメタ情報を持てるようにする


2. ソート条件を Func<T, object> で渡せる柔軟なカスタムソート対応


3. 出力形式を Dictionary<string, List<T>> にして Meta をキーにする


4. 残りアイテムの自動グルーピング or 補足パーティション作成（Count合計未満の場合）




---

他にも必要な機能があれば教えてください！次に進める案として「カスタムソートキー」や「分割後の変換処理（map）」なども追加可能です。





値変更時や行追加時に C1FlexGrid を更新する方法を考慮し、適切に処理を行うように修正します。


---

対応方法

1. 値変更時に更新

ObservableCollection<DataItem> の CollectionChanged イベントを利用。

INotifyPropertyChanged の PropertyChanged を監視し、値変更を検知。



2. 行追加時に更新

ObservableCollection<DataItem> の CollectionChanged イベントで行追加を検知。





---

修正後のコード

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;

public partial class MainForm : Form
{
    private C1FlexGrid flexGrid;
    private ObservableCollection<DataItem> data;

    public MainForm()
    {
        InitializeComponent();
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        // C1FlexGrid の初期化
        flexGrid = new C1FlexGrid
        {
            Dock = DockStyle.Fill,
            DrawMode = DrawModeEnum.OwnerDraw,
            AllowAddNew = true // 行追加を許可
        };
        Controls.Add(flexGrid);

        // データソースの作成
        data = new ObservableCollection<DataItem>
        {
            new DataItem { Name = "Item 1", IsEditable = true },
            new DataItem { Name = "Item 2", IsEditable = false },
            new DataItem { Name = "Item 3", IsEditable = true },
        };

        // データ変更時の更新を監視
        data.CollectionChanged += Data_CollectionChanged;
        foreach (var item in data)
        {
            item.PropertyChanged += DataItem_PropertyChanged;
        }

        flexGrid.DataSource = data;

        // イベントハンドラ設定
        flexGrid.OwnerDrawCell += FlexGrid_OwnerDrawCell;
        flexGrid.StartEdit += FlexGrid_StartEdit;
    }

    private void Data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            // 新しく追加されたアイテムに PropertyChanged イベントを登録
            foreach (var newItem in e.NewItems.OfType<DataItem>())
            {
                newItem.PropertyChanged += DataItem_PropertyChanged;
            }
        }
        flexGrid.Refresh();
    }

    private void DataItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DataItem.IsEditable))
        {
            flexGrid.Refresh(); // IsEditable の変更時にセルの描画を更新
        }
    }

    private void FlexGrid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
    {
        if (e.Row < flexGrid.Rows.Fixed || e.Col < flexGrid.Cols.Fixed) return;

        var item = flexGrid.Rows[e.Row].DataSource as DataItem;
        if (item == null) return;

        if (!item.IsEditable)
        {
            e.Style.BackColor = Color.LightGray;
            e.Style.ForeColor = Color.DarkGray;
        }
    }

    private void FlexGrid_StartEdit(object sender, RowColEventArgs e)
    {
        var item = flexGrid.Rows[e.Row].DataSource as DataItem;
        if (item != null && !item.IsEditable)
        {
            e.Cancel = true; // 編集不可
        }
    }
}


---

ポイント

1. ObservableCollection<T>.CollectionChanged イベント

行が追加されたときに PropertyChanged を登録し、動的に更新を適用。



2. INotifyPropertyChanged の監視

IsEditable の変更時に flexGrid.Refresh() を実行し、セルの表示を更新。



3. 新規行の追加

AllowAddNew = true を設定し、ユーザーが新しい行を追加可能にする。





---

拡張案

1. 削除時の処理追加

CollectionChanged 内で Remove の場合、PropertyChanged の登録解除を行う。


if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
{
    foreach (var oldItem in e.OldItems.OfType<DataItem>())
    {
        oldItem.PropertyChanged -= DataItem_PropertyChanged;
    }
}


2. 新規行のデフォルト設定

NewRow にデフォルト値を設定する。


flexGrid.AfterRowColChange += (s, e) =>
{
    if (flexGrid.Row == flexGrid.Rows.Count - 1) // 最終行（新規行）
    {
        var item = flexGrid.Rows[flexGrid.Row].DataSource as DataItem;
        if (item != null)
        {
            item.IsEditable = true; // 追加時はデフォルトで編集可能
        }
    }
};




---

まとめ

ObservableCollection<T> の CollectionChanged を利用し、行追加時にイベントを登録。

PropertyChanged を利用し、値変更時に Refresh() を実行。

OwnerDrawCell でセルのスタイルを変更し、StartEdit で入力制御。


この実装で、新しい行の追加や既存データの変更時にも、適切にグリッドを更新できるようになります！




SettingsManager を シングルトン化 して、アプリ全体で共有できるようにします。
これにより、どこからでも同じ SettingsManager インスタンスにアクセスでき、設定のリアルタイム同期を一元的に管理できます。


---

シングルトン化した SettingsManager

実装のポイント

1. スレッドセーフなシングルトン を Lazy<T> で実装。


2. グローバルアクセス用の Instance プロパティ を追加。


3. コンストラクタを private にしてインスタンスの直接作成を防ぐ。




---

修正後の SettingsManager

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class SettingsManager
{
    private static readonly Lazy<SettingsManager> _instance = new(() => new SettingsManager());

    public static SettingsManager Instance => _instance.Value;

    private readonly Dictionary<Type, ISettings> _settings = new();
    private readonly Dictionary<Type, FileSystemWatcher> _watchers = new();

    public event Action<Type, ISettings>? SettingsUpdated;

    private SettingsManager() { }

    public T GetSettings<T>() where T : ISettings, new()
    {
        var type = typeof(T);
        if (!_settings.TryGetValue(type, out var settings))
        {
            settings = LoadSettings<T>();
            _settings[type] = settings;
            WatchFileChanges<T>();
        }
        return (T)settings;
    }

    public void UpdateSettings<T>(T newSettings) where T : ISettings
    {
        var type = typeof(T);
        _settings[type] = newSettings;
        SaveSettings(newSettings);
    }

    private T LoadSettings<T>() where T : ISettings, new()
    {
        string filePath = GetFilePath<T>();
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }
        return new T();
    }

    private void SaveSettings<T>(T settings) where T : ISettings
    {
        string filePath = GetFilePath<T>();
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    private void WatchFileChanges<T>() where T : ISettings, new()
    {
        string filePath = GetFilePath<T>();
        string directory = Path.GetDirectoryName(filePath) ?? ".";
        string fileName = Path.GetFileName(filePath);

        var watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite
        };

        watcher.Changed += (sender, e) =>
        {
            System.Threading.Thread.Sleep(100); // 書き込み完了を待つ
            var newSettings = LoadSettings<T>();
            _settings[typeof(T)] = newSettings;
            SettingsUpdated?.Invoke(typeof(T), newSettings);
            Console.WriteLine($"Settings updated: {typeof(T).Name}");
        };

        watcher.EnableRaisingEvents = true;
        _watchers[typeof(T)] = watcher;
    }

    private string GetFilePath<T>() => $"{typeof(T).Name}.json";
}


---

シングルトン化による使用方法

class Program
{
    static void Main()
    {
        // シングルトンインスタンスを取得
        var settingsManager = SettingsManager.Instance;

        // 設定変更時の通知を受け取る
        settingsManager.SettingsUpdated += (type, settings) =>
        {
            Console.WriteLine($"[Notification] {type.Name} settings have been updated!");
        };

        // 設定を取得
        var generalSettings = settingsManager.GetSettings<GeneralSettings>();
        Console.WriteLine($"Language: {generalSettings.Language}");

        var networkSettings = settingsManager.GetSettings<NetworkSettings>();
        Console.WriteLine($"ServerIP: {networkSettings.ServerIP}, Port: {networkSettings.Port}");

        // 設定を更新（ファイルも変更される）
        generalSettings.Language = "Japanese";
        settingsManager.UpdateSettings(generalSettings);

        networkSettings.Port = 9090;
        settingsManager.UpdateSettings(networkSettings);

        Console.WriteLine("Waiting for file changes...");
        Console.ReadLine(); // プログラムが終了しないように待機
    }
}


---

シングルトン化のメリット

1. アプリケーション全体で設定を統一的に管理

どこからでも SettingsManager.Instance を呼び出せる。



2. リソースの節約

FileSystemWatcher のインスタンスをアプリ内で一元管理できる。



3. リアルタイム同期が確実に機能

どこから設定を変更しても、すべてのコンポーネントに通知が届く。





---

さらなる拡張

設定のリモート同期

WebSocket を使って複数のクライアント間で設定を同期。


設定のキャッシュ機能

頻繁にアクセスされる設定をキャッシュし、パフォーマンスを向上。


GUIアプリとの連携

設定変更イベントを UI に即反映（例: WPF, WinForms）。




---

この方法で、シングルトンでリアルタイム同期可能な設定管理クラスを実現できます！





リアルタイム同期機能を追加するために、FileSystemWatcher を使用して JSON ファイルの変更を監視し、設定の変更を即時反映するように拡張します。


---

実装方針

1. FileSystemWatcher を使って設定ファイルの変更を監視する。


2. 設定ファイルが更新されたら自動で再読み込みする。


3. 設定が変更された際にイベントを発火し、他のコンポーネントが変更を受け取れるようにする。




---

拡張したコード

1. 設定変更通知用のインターフェース

public interface ISettings
{
}

すべての設定クラスが ISettings を実装するようにすることで、統一的に扱えるようにします。


---

2. 設定クラスの変更

各設定クラスに ISettings を適用。

public class GeneralSettings : ISettings
{
    public string Language { get; set; } = "English";
    public bool EnableLogging { get; set; } = true;
}

public class NetworkSettings : ISettings
{
    public string ServerIP { get; set; } = "192.168.1.1";
    public int Port { get; set; } = 8080;
}

public class DisplaySettings : ISettings
{
    public int Brightness { get; set; } = 50;
    public bool DarkMode { get; set; } = false;
}


---

3. 設定マネージャの拡張

設定ファイルの監視を FileSystemWatcher で実装。

設定が変更されたらイベントを発火して通知。


using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class SettingsManager
{
    private readonly Dictionary<Type, ISettings> _settings = new();
    private readonly Dictionary<Type, FileSystemWatcher> _watchers = new();
    
    public event Action<Type, ISettings>? SettingsUpdated;

    public T GetSettings<T>() where T : ISettings, new()
    {
        var type = typeof(T);
        if (!_settings.TryGetValue(type, out var settings))
        {
            settings = LoadSettings<T>();
            _settings[type] = settings;
            WatchFileChanges<T>();
        }
        return (T)settings;
    }

    public void UpdateSettings<T>(T newSettings) where T : ISettings
    {
        var type = typeof(T);
        _settings[type] = newSettings;
        SaveSettings(newSettings);
    }

    private T LoadSettings<T>() where T : ISettings, new()
    {
        string filePath = GetFilePath<T>();
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }
        return new T();
    }

    private void SaveSettings<T>(T settings) where T : ISettings
    {
        string filePath = GetFilePath<T>();
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    private void WatchFileChanges<T>() where T : ISettings, new()
    {
        string filePath = GetFilePath<T>();
        string directory = Path.GetDirectoryName(filePath) ?? ".";
        string fileName = Path.GetFileName(filePath);

        var watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite
        };

        watcher.Changed += (sender, e) =>
        {
            System.Threading.Thread.Sleep(100); // ファイル書き込みの完了を待つ
            var newSettings = LoadSettings<T>();
            _settings[typeof(T)] = newSettings;
            SettingsUpdated?.Invoke(typeof(T), newSettings);
            Console.WriteLine($"Settings updated: {typeof(T).Name}");
        };

        watcher.EnableRaisingEvents = true;
        _watchers[typeof(T)] = watcher;
    }

    private string GetFilePath<T>() => $"{typeof(T).Name}.json";
}


---

4. 設定変更時の通知を受け取る

クラスが SettingsManager.SettingsUpdated イベントを購読すれば、設定が変更されたときにリアルタイムで通知を受け取ることができます。

class Program
{
    static void Main()
    {
        var settingsManager = new SettingsManager();

        // 設定が更新されたら通知
        settingsManager.SettingsUpdated += (type, settings) =>
        {
            Console.WriteLine($"[Notification] {type.Name} settings have been updated!");
        };

        // 設定を取得（初回ロード時に監視を開始）
        var generalSettings = settingsManager.GetSettings<GeneralSettings>();
        Console.WriteLine($"Language: {generalSettings.Language}");

        var networkSettings = settingsManager.GetSettings<NetworkSettings>();
        Console.WriteLine($"ServerIP: {networkSettings.ServerIP}, Port: {networkSettings.Port}");

        // 設定を更新（ファイルも変更される）
        generalSettings.Language = "Japanese";
        settingsManager.UpdateSettings(generalSettings);

        networkSettings.Port = 9090;
        settingsManager.UpdateSettings(networkSettings);

        Console.WriteLine("Waiting for file changes...");
        Console.ReadLine(); // プログラムが終了しないように待機
    }
}


---

動作

1. 最初の設定ロード時に FileSystemWatcher を起動。


2. 設定ファイルが変更されたら即時に再ロードし、イベントを発火。


3. コンソールに設定変更の通知が表示される。




---

さらに強化

1. 変更検出の最適化

ファイル変更を即座に処理するのではなく、短時間の変更をバッチ処理する（例: 500ms 以内の変更はまとめる）。



2. データベース連携

設定の保存を SQLite にすることで、データの整合性を高める。



3. クラウド同期

Firebase や WebSocket を利用して、複数のアプリ間でリアルタイム同期。





---

まとめ

FileSystemWatcher を活用し、設定変更をリアルタイムで検出。

SettingsUpdated イベントを利用して、アプリケーション全体で変更を通知。

ISettings インターフェースを導入し、統一的な管理を実現。


これにより、設定変更がリアルタイムで適用されるシステムを構築できます！




WinForms で、ラジオボタン（RadioButton）のチェック時とアンチェック時の描画をカスタマイズするには、RadioButton を継承して OnPaint をオーバーライドするのが一般的です。

カスタム RadioButton コントロール

以下のコードは、カスタム RadioButton を作成し、チェック時とアンチェック時で異なる描画を行うものです。

手順

1. CustomRadioButton クラスを作成し、RadioButton を継承。


2. OnPaint をオーバーライドし、チェック時とアンチェック時で異なる描画を行う。


3. Invalidate() を使って、CheckedChanged イベントで再描画。



カスタム RadioButton コード

using System;
using System.Drawing;
using System.Windows.Forms;

public class CustomRadioButton : RadioButton
{
    public Color CheckedColor { get; set; } = Color.Green;
    public Color UncheckedColor { get; set; } = Color.Gray;
    public Color BorderColor { get; set; } = Color.Black;
    public int CircleSize { get; set; } = 16;

    public CustomRadioButton()
    {
        this.AutoSize = false;
        this.Width = 120;
        this.Height = 24;
    }

    protected override void OnCheckedChanged(EventArgs e)
    {
        base.OnCheckedChanged(e);
        this.Invalidate(); // 状態変更時に再描画
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int radius = CircleSize;
        int padding = 4;
        Rectangle outerRect = new Rectangle(0, (Height - radius) / 2, radius, radius);
        Rectangle innerRect = new Rectangle(outerRect.X + padding, outerRect.Y + padding, radius - 2 * padding, radius - 2 * padding);

        // 外枠
        using (Pen borderPen = new Pen(BorderColor, 2))
        {
            g.DrawEllipse(borderPen, outerRect);
        }

        // 塗りつぶし（チェック時）
        if (Checked)
        {
            using (Brush checkBrush = new SolidBrush(CheckedColor))
            {
                g.FillEllipse(checkBrush, innerRect);
            }
        }
        else
        {
            using (Brush uncheckBrush = new SolidBrush(UncheckedColor))
            {
                g.FillEllipse(uncheckBrush, innerRect);
            }
        }

        // テキスト描画
        using (Brush textBrush = new SolidBrush(this.ForeColor))
        {
            g.DrawString(this.Text, this.Font, textBrush, radius + 8, (Height - Font.Height) / 2);
        }
    }
}


---

使用方法

1. Form1.cs のデザイナから普通の RadioButton を配置。


2. Form1.cs のコードで CustomRadioButton を追加。



public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        
        var radio1 = new CustomRadioButton
        {
            Text = "Option 1",
            Location = new Point(20, 20),
            Checked = true
        };

        var radio2 = new CustomRadioButton
        {
            Text = "Option 2",
            Location = new Point(20, 50)
        };

        this.Controls.Add(radio1);
        this.Controls.Add(radio2);
    }
}


---

ポイント

OnCheckedChanged で Invalidate() を呼び出し、状態変更時に再描画。

OnPaint で Graphics を使い、外枠と内部の塗りつぶしを変更。

Checked の状態によって塗りつぶし色を変更。


このカスタム RadioButton は、チェック時とアンチェック時の描画を自由に変更できます。






C# でのメッセンジャーアプリ開発の流れは以下のようになります。

1. システム概要

ファイル送信: 送信者がファイルを選択し、送信先を指定してアップロード

通知機能: 受信者に新着ファイルの通知を送る

ファイル管理: サーバー上のフォルダに保存し、受信者がダウンロード可能にする

受信処理: 受信者がファイルをダウンロードし、ステータスを完了にする


2. 技術構成

フロントエンド（UI）: WinForms または WPF（好みによる）

バックエンド（API）: ASP.NET Core（REST API または SignalR）

データベース: SQLite / PostgreSQL / SQL Server（ファイルメタ情報の管理）

ファイル保存: ローカルサーバーのフォルダ / クラウドストレージ

リアルタイム通知: SignalR または WebSocket



---

3. 具体的な開発内容

(1) ファイル送信

送信時に、ファイルを選択し、送信相手を指定してアップロードします。

private async Task UploadFile(string filePath, string recipient)
{
    using var client = new HttpClient();
    using var content = new MultipartFormDataContent();
    var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

    content.Add(fileContent, "file", Path.GetFileName(filePath));
    content.Add(new StringContent(recipient), "recipient");

    var response = await client.PostAsync("https://yourserver.com/api/upload", content);
    if (response.IsSuccessStatusCode)
    {
        MessageBox.Show("ファイル送信完了");
    }
}

(2) サーバー側のファイル保存

ASP.NET Core の API を使用して、ファイルをサーバーのフォルダに保存します。

[HttpPost("upload")]
public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string recipient)
{
    if (file == null || file.Length == 0)
    {
        return BadRequest("ファイルが選択されていません");
    }

    var filePath = Path.Combine("C:\\ServerFiles", file.FileName);
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    // データベースにファイル情報を保存（省略）

    // 受信者に通知（SignalR）
    await _hubContext.Clients.User(recipient).SendAsync("ReceiveNotification", file.FileName);

    return Ok(new { Message = "ファイルアップロード完了" });
}

(3) 受信通知

SignalR を使い、リアルタイムで受信通知を表示します。

public class NotificationHub : Hub
{
    public async Task SendNotification(string user, string message)
    {
        await Clients.User(user).SendAsync("ReceiveNotification", message);
    }
}

クライアント側（WinForms）で受信処理:

var connection = new HubConnectionBuilder()
    .WithUrl("https://yourserver.com/notificationHub")
    .Build();

connection.On<string>("ReceiveNotification", (message) =>
{
    MessageBox.Show($"新着ファイル: {message}");
});

await connection.StartAsync();

(4) 受信時のダウンロード処理

受信者が「受け取る」ボタンを押すと、ファイルをローカルにダウンロードします。

private async Task DownloadFile(string fileName)
{
    using var client = new HttpClient();
    var response = await client.GetAsync($"https://yourserver.com/api/download?fileName={fileName}");
    
    if (response.IsSuccessStatusCode)
    {
        var fileBytes = await response.Content.ReadAsByteArrayAsync();
        File.WriteAllBytes(Path.Combine("C:\\LocalFiles", fileName), fileBytes);
        MessageBox.Show("ファイル受信完了");

        // ステータスを完了に変更（APIリクエスト）
        await client.PostAsync($"https://yourserver.com/api/complete?fileName={fileName}", null);
    }
}

(5) ステータス更新

受信完了後、データベースのステータスを「完了」に変更します。

[HttpPost("complete")]
public IActionResult MarkAsComplete([FromQuery] string fileName)
{
    // データベースでステータスを「完了」に更新（省略）
    return Ok();
}


---

4. 実装のポイント

認証・認可: ユーザー管理は JWT 認証を導入

エラーハンドリング: ファイルの重複や削除対応

UI の工夫: 受信ファイルのリストを表示


このように設計すれば、シンプルで使いやすいファイル送受信メッセンジャーアプリが作れます。さらに詳細な仕様があれば教えてください！






DataTable の各行に対する操作を高速化し、汎用的に利用できるメソッドを作成します。以下の要件を満たすように設計します。

要件

1. 並列処理をサポート


2. 型の安全性を確保


3. 汎用的に使用可能


4. 高速化のため Span<T> などを活用


5. イベントオーバーヘッドを抑制


6. スレッドセーフな方法を選択




---

汎用メソッドの実装

以下の ProcessDataTable メソッドは、任意の Action<DataRow> を適用しつつ DataTable の操作を高速化します。

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public static class DataTableExtensions
{
    /// <summary>
    /// DataTable の各行に対する処理を並列化しつつ高速に実行する汎用メソッド
    /// </summary>
    /// <param name="table">処理対象の DataTable</param>
    /// <param name="action">各 DataRow に適用する処理</param>
    /// <param name="parallel">並列処理を有効にするか</param>
    public static void ProcessDataTable(this DataTable table, Action<DataRow> action, bool parallel = true)
    {
        if (table == null || action == null) throw new ArgumentNullException();

        // DataRow を直接操作すると遅いため、配列に変換してから処理
        var rows = table.Select(); 

        // イベントを無効化して高速化
        table.BeginLoadData();

        if (parallel)
        {
            Parallel.ForEach(rows, row =>
            {
                action(row);
            });
        }
        else
        {
            foreach (var row in rows)
            {
                action(row);
            }
        }

        // イベント再開
        table.EndLoadData();
    }

    /// <summary>
    /// DataTable を List<T> に変換し、高速処理後に再適用するメソッド
    /// </summary>
    /// <typeparam name="T">変換するデータ型</typeparam>
    /// <param name="table">処理対象の DataTable</param>
    /// <param name="selector">DataRow から T への変換関数</param>
    /// <param name="mutator">T に対する変更処理</param>
    /// <param name="applyBack">処理後に DataTable へ戻すか</param>
    public static void ProcessDataTableAsList<T>(
        this DataTable table,
        Func<DataRow, T> selector,
        Action<T> mutator,
        Action<DataRow, T>? applyBack = null)
    {
        if (table == null || selector == null || mutator == null) throw new ArgumentNullException();

        // DataTable を List<T> に変換
        List<T> data = table.AsEnumerable().Select(selector).ToList();

        // `Span<T>` で最適化
        var span = CollectionsMarshal.AsSpan(data);
        Parallel.For(0, span.Length, i =>
        {
            mutator(span[i]);
        });

        // DataTable に適用
        if (applyBack != null)
        {
            int index = 0;
            foreach (var row in table.Rows.Cast<DataRow>())
            {
                applyBack(row, data[index++]);
            }
        }
    }
}


---

汎用メソッドの使用例

1. DataTable の各行を並列処理

DataTable table = new DataTable();
table.Columns.Add("ID", typeof(int));
table.Columns.Add("Value", typeof(string));

for (int i = 0; i < 10000; i++)
{
    table.Rows.Add(i, $"Value {i}");
}

// 各行の "Value" を大文字に変換（並列処理）
table.ProcessDataTable(row =>
{
    row["Value"] = ((string)row["Value"]).ToUpper();
});

✅ ポイント

各 DataRow に対して Action<DataRow> を適用

Parallel.ForEach により並列処理を実行

BeginLoadData() によりオーバーヘッドを削減



---

2. List<T> に変換して処理

// List<(int ID, string Value)> に変換して並列処理
table.ProcessDataTableAsList(
    row => (row.Field<int>("ID"), row.Field<string>("Value")),
    entry => entry = (entry.ID, entry.Value.ToLower()), // 小文字変換
    (row, entry) => row["Value"] = entry.Value
);

✅ ポイント

DataRow ではなく List<T> に変換して操作するため DataRow のオーバーヘッドを回避

Span<T> を活用してメモリ管理を最適化

applyBack により DataTable に適用



---

パフォーマンス比較


---

結論

1. ProcessDataTable()

汎用的で簡単に適用可能

並列処理が可能

オーバーヘッドを抑制



2. ProcessDataTableAsList<T>()

データを List<T> に変換し、高速処理

Span<T> でメモリ効率向上

データの書き戻しもサポート




これらのメソッドを活用すれば、DataTable の行処理を 劇的に高速化 できます！




確かに、画像数が多いとスレッドプールが枯渇し、アプリのレスポンスが悪化する可能性があります。
そのため、スレッドプールの過負荷を防ぐために「一定数ずつ画像を読み込む」制御 を入れましょう。


---

改善点

✅ プレースホルダー（Loading...）は即表示
✅ 画像のロードは並列処理だが、同時に処理する画像数を制限（スレッドプールの枯渇防止）
✅ 画像を少しずつ読み込みながら UI に反映
✅ 最適なスレッド数 (MaxDegreeOfParallelism) を指定


---

1. SelectableImageControl（変更なし）

プレースホルダーと画像のロード機能を持った UserControl です。

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

public class SelectableImageControl : UserControl, ISelectableItem
{
    private PictureBox pictureBox;
    private Label placeholderLabel;
    private bool isSelected = false;
    private string imagePath;

    public object Value => imagePath; // 選択された画像のパス

    public SelectableImageControl(string imagePath)
    {
        this.imagePath = imagePath;
        this.Size = new Size(120, 120);
        this.BorderStyle = BorderStyle.FixedSingle;
        this.BackColor = Color.LightGray;

        // プレースホルダー（画像未読込時）
        placeholderLabel = new Label
        {
            Text = "Loading...",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 画像表示用の PictureBox
        pictureBox = new PictureBox
        {
            SizeMode = PictureBoxSizeMode.Zoom,
            Dock = DockStyle.Fill,
            Visible = false // 最初は非表示
        };

        this.Controls.Add(pictureBox);
        this.Controls.Add(placeholderLabel);
        this.Click += (s, e) => SelectItem(); // クリックで選択
    }

    public async Task LoadImageAsync()
    {
        try
        {
            Image img = await Task.Run(() => Image.FromFile(imagePath));

            // UIスレッドで画像を表示
            this.Invoke((Action)(() =>
            {
                pictureBox.Image = img;
                pictureBox.Visible = true;
                placeholderLabel.Visible = false; // プレースホルダーを非表示
            }));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"画像の読み込みに失敗しました: {ex.Message}");
        }
    }

    public void SelectItem()
    {
        this.BackColor = Color.LightBlue;
        isSelected = true;
    }

    public void DeselectItem()
    {
        this.BackColor = Color.LightGray;
        isSelected = false;
    }
}


---

2. SelectionDialog（画像のロードを制御）

ここで 並列処理の制御を追加 します。

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

public class SelectionDialog : Form
{
    private FlowLayoutPanel flowLayoutPanel;
    private Button okButton;
    private Button cancelButton;
    private ISelectableItem selectedItem = null;
    private const int MaxConcurrentLoads = 4; // 画像の同時ロード数（調整可能）

    public ISelectableItem SelectedItem => selectedItem; // 選択されたアイテムを取得

    public SelectionDialog(List<SelectableImageControl> items)
    {
        this.Text = "画像選択";
        this.Size = new Size(600, 400);

        // FlowLayoutPanel 設定
        flowLayoutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(10)
        };

        // ボタンの設定
        okButton = new Button { Text = "OK", Dock = DockStyle.Bottom, Enabled = false };
        cancelButton = new Button { Text = "キャンセル", Dock = DockStyle.Bottom };

        okButton.Click += (s, e) => this.DialogResult = DialogResult.OK;
        cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        // プレースホルダーを先に追加
        foreach (var item in items)
        {
            item.Click += (s, e) => SelectItem(item);
            flowLayoutPanel.Controls.Add(item);
        }

        // フォームに追加
        this.Controls.Add(flowLayoutPanel);
        this.Controls.Add(okButton);
        this.Controls.Add(cancelButton);

        // 画像を並列ロード（最大 `MaxConcurrentLoads` ずつ）
        _ = LoadImagesAsync(items);
    }

    private async Task LoadImagesAsync(List<SelectableImageControl> items)
    {
        using (SemaphoreSlim semaphore = new SemaphoreSlim(MaxConcurrentLoads))
        {
            List<Task> tasks = new List<Task>();

            foreach (var item in items)
            {
                await semaphore.WaitAsync(); // 同時にロードする数を制限

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await item.LoadImageAsync();
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks); // すべてのロードが終わるまで待機
        }
    }

    private void SelectItem(ISelectableItem item)
    {
        foreach (Control control in flowLayoutPanel.Controls)
        {
            if (control is ISelectableItem selectable)
            {
                selectable.DeselectItem();
            }
        }

        item.SelectItem();
        selectedItem = item;
        okButton.Enabled = true;
    }
}


---

3. MainForm（変更なし）

フォルダを選択し、画像を SelectionDialog に渡す処理。

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public class MainForm : Form
{
    private Button openDialogButton;
    private Label selectedImageLabel;

    public MainForm()
    {
        this.Text = "画像選択ダイアログ";
        this.Size = new System.Drawing.Size(400, 200);

        openDialogButton = new Button
        {
            Text = "画像選択ダイアログを開く",
            Dock = DockStyle.Top
        };

        selectedImageLabel = new Label
        {
            Text = "選択された画像: なし",
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };

        openDialogButton.Click += OpenSelectionDialog;

        this.Controls.Add(selectedImageLabel);
        this.Controls.Add(openDialogButton);
    }

    private void OpenSelectionDialog(object sender, EventArgs e)
    {
        using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
        {
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                List<SelectableImageControl> items = Directory.GetFiles(folderDialog.SelectedPath, "*.*")
                    .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    .Select(path => new SelectableImageControl(path))
                    .ToList();

                using (SelectionDialog dialog = new SelectionDialog(items))
                {
                    if (dialog.ShowDialog() == DialogResult.OK && dialog.SelectedItem != null)
                    {
                        selectedImageLabel.Text = $"選択された画像: {dialog.SelectedItem.Value}";
                    }
                }
            }
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}


---

改善点

✅ スレッドプールの枯渇を防ぐ（最大 4 つの画像を同時ロード）
✅ UI は即座に反映し、"Loading..." を表示
✅ 画像のロードが完了したものから即時表示
✅ 大量の画像がある場合もスムーズに動作


---

動作フロー

1. 画像のプレースホルダーが即座に表示（"Loading..."）


2. 最大 4 つずつ並列で画像をロードし、終わったものから更新


3. ユーザーが画像をクリック → 選択状態になる


4. OK で選択結果を返す



この方式なら、スレッドプールを無駄に消費せず、スムーズに画像を読み込めます！

