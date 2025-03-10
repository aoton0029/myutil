MenuBarManager にプロジェクトモデルクラスを追加し、変更を監視しながら Undo/Redo 機能を実装します。


---

実装ポイント

1. ProjectModel の作成

プロジェクトの状態を保持するクラスを作成し、変更を監視。



2. Undo/Redo の実装

Stack<ProjectModel> を使用して、履歴を管理し Undo/Redo を可能にする。



3. MenuBarManager に統合

ProjectModel の変更を検知し、ファイル操作と連携。





---

1. ProjectModel（プロジェクトデータを管理）

using System;
using System.ComponentModel;

public class ProjectModel : INotifyPropertyChanged, ICloneable
{
    private string _content = "";

    public string Content
    {
        get => _content;
        set
        {
            if (_content != value)
            {
                _content = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public object Clone()
    {
        return new ProjectModel { Content = this.Content };
    }
}

ポイント

INotifyPropertyChanged を実装し、変更を検知。

Clone() メソッドでオブジェクトの複製を作成し、Undo/Redo に使用。



---

2. MenuBarManager の実装

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

public class MenuBarManager
{
    public event EventHandler<ProjectModel>? FileOpened;
    public event EventHandler<string>? FileSaved;
    public event EventHandler? NewFileCreated;
    public event EventHandler? ApplicationExited;

    private string? currentFilePath;
    private ProjectModel currentProject = new ProjectModel();

    private readonly Stack<ProjectModel> undoStack = new();
    private readonly Stack<ProjectModel> redoStack = new();

    public MenuBarManager()
    {
        currentProject.PropertyChanged += (s, e) => SaveUndoState();
    }

    public void NewFile()
    {
        currentFilePath = null;
        currentProject = new ProjectModel();
        SaveUndoState();
        NewFileCreated?.Invoke(this, EventArgs.Empty);
    }

    public void OpenFile()
    {
        using OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            Title = "ファイルを開く"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            currentFilePath = openFileDialog.FileName;
            currentProject.Content = File.ReadAllText(currentFilePath);
            SaveUndoState();
            FileOpened?.Invoke(this, currentProject);
        }
    }

    public void SaveFile()
    {
        if (string.IsNullOrEmpty(currentFilePath))
        {
            SaveFileAs();
        }
        else
        {
            File.WriteAllText(currentFilePath, currentProject.Content);
            FileSaved?.Invoke(this, currentFilePath);
        }
    }

    public void SaveFileAs()
    {
        using SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            Title = "名前を付けて保存"
        };

        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            currentFilePath = saveFileDialog.FileName;
            File.WriteAllText(currentFilePath, currentProject.Content);
            FileSaved?.Invoke(this, currentFilePath);
        }
    }

    private void SaveUndoState()
    {
        undoStack.Push((ProjectModel)currentProject.Clone());
        redoStack.Clear();
    }

    public void Undo()
    {
        if (undoStack.Count > 1)
        {
            redoStack.Push(undoStack.Pop());
            currentProject = (ProjectModel)undoStack.Peek().Clone();
            FileOpened?.Invoke(this, currentProject);
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            undoStack.Push(redoStack.Pop());
            currentProject = (ProjectModel)undoStack.Peek().Clone();
            FileOpened?.Invoke(this, currentProject);
        }
    }

    public void ExitApplication()
    {
        ApplicationExited?.Invoke(this, EventArgs.Empty);
        Application.Exit();
    }
}


---

拡張案

1. 複数のプロジェクトファイルを管理

Dictionary<string, ProjectModel> を使用し、複数ファイルをタブで管理。



2. プロジェクトにメタデータを追加

Title, LastModified, CreatedDate などを追加。



3. 変更がある場合、アプリ終了前に保存確認

MessageBox.Show() を使って保存確認を表示。



4. Undo/Redo の上限設定

Stack のサイズを制限し、古い履歴を削除。



5. JSON ベースのプロジェクト保存

System.Text.Json を使って保存/読み込みをJSON化。





---

これで Undo/Redo が機能する MenuBarManager を作成できました！ UI 側と連携して、より使いやすくしていけます。




C#のWinFormsやWPFでメニューバーの「ファイル」タブの機能をまとめるために、専用のクラスを作成すると管理しやすくなります。

---

### **構成**
1. **`FileMenuHandler` クラス**  
   - 「ファイル」メニューのイベントを管理するクラス。
   - 新規作成、開く、保存、終了 などの機能を提供。

2. **`IMainForm` インターフェース**  
   - メニュー操作で影響を受けるメインフォームの操作を統一するためのインターフェース。

3. **WinFormsでの利用例**  
   - `FileMenuHandler` をフォームに統合。

---

### **実装**

#### **1. `IMainForm` インターフェース**
メインフォームで必要な操作を統一する。

```csharp
public interface IMainForm
{
    void CreateNewFile();
    void OpenFile();
    void SaveFile();
    void ExitApplication();
}
```

---

#### **2. `FileMenuHandler` クラス**
メニューの動作をカプセル化。

```csharp
using System;
using System.Windows.Forms;

public class FileMenuHandler
{
    private readonly IMainForm _mainForm;

    public FileMenuHandler(IMainForm mainForm)
    {
        _mainForm = mainForm;
    }

    public void HandleMenuClick(object sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem menuItem)
        {
            switch (menuItem.Name)
            {
                case "newFileMenuItem":
                    _mainForm.CreateNewFile();
                    break;
                case "openFileMenuItem":
                    _mainForm.OpenFile();
                    break;
                case "saveFileMenuItem":
                    _mainForm.SaveFile();
                    break;
                case "exitMenuItem":
                    _mainForm.ExitApplication();
                    break;
            }
        }
    }
}
```

---

#### **3. メインフォームでの利用**
`FileMenuHandler` を統合し、フォームの機能を実装。

```csharp
public partial class MainForm : Form, IMainForm
{
    private FileMenuHandler _fileMenuHandler;

    public MainForm()
    {
        InitializeComponent();
        _fileMenuHandler = new FileMenuHandler(this);

        // イベントハンドラをセット
        newFileMenuItem.Click += _fileMenuHandler.HandleMenuClick;
        openFileMenuItem.Click += _fileMenuHandler.HandleMenuClick;
        saveFileMenuItem.Click += _fileMenuHandler.HandleMenuClick;
        exitMenuItem.Click += _fileMenuHandler.HandleMenuClick;
    }

    public void CreateNewFile()
    {
        MessageBox.Show("新規ファイル作成");
    }

    public void OpenFile()
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show($"開いたファイル: {openFileDialog.FileName}");
            }
        }
    }

    public void SaveFile()
    {
        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show($"保存したファイル: {saveFileDialog.FileName}");
            }
        }
    }

    public void ExitApplication()
    {
        Application.Exit();
    }
}
```

---

### **拡張案**
1. **最近開いたファイル機能**
   - `Properties.Settings` を活用し、最近開いたファイルを保存・表示。

2. **ショートカットキーのサポート**
   - `KeyPreview = true` を設定し、`KeyDown` イベントで `Ctrl + S` などのショートカットを処理。

3. **プラグイン対応**
   - メニューを動的に変更できるよう `Action` デリゲートを使う。

---

このように `FileMenuHandler` を作成することで、コードの見通しが良くなり、拡張もしやすくなります！



WinForms のメニューバーを汎用的に設計し、異なるフォームやアプリケーションで再利用できるアーキテクチャを構築する方法を考えます。

## 設計方針
1. **メニューバーを `UserControl` として作成**
   - 共通のメニュー構成を `UserControl` にカプセル化する。
   - 各メニュー項目を動的に変更可能にする。

2. **メニュー項目の設定を外部ファイル（JSONなど）で管理**
   - メニュー項目の名前やイベントハンドラを設定ファイルから読み込む。
   - 柔軟にメニュー構成を変更可能にする。

3. **コマンドパターンを使用して動作を切り替え**
   - `ICommand` インターフェースを定義し、各メニューの動作を実装する。
   - 設定ファイルでどのコマンドを実行するか指定できるようにする。

---

## 実装

### 1. メニューの定義 (`MenuItemConfig.cs`)
まず、メニューの構成を表すクラスを作成します。

```csharp
public class MenuItemConfig
{
    public string Name { get; set; }
    public string Command { get; set; } // 実行するコマンドのキー
    public List<MenuItemConfig> SubItems { get; set; } = new List<MenuItemConfig>();
}
```

---

### 2. コマンドパターン (`ICommand.cs`)
コマンドの実装を汎用化し、動作を柔軟に変更可能にします。

```csharp
public interface ICommand
{
    void Execute();
}
```

各メニューのアクションをコマンドとして定義します。

```csharp
public class OpenFileCommand : ICommand
{
    public void Execute()
    {
        MessageBox.Show("ファイルを開く処理");
    }
}

public class ExitCommand : ICommand
{
    public void Execute()
    {
        Application.Exit();
    }
}
```

コマンドを管理するクラスを用意します。

```csharp
public class CommandRegistry
{
    private Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

    public void Register(string key, ICommand command)
    {
        commands[key] = command;
    }

    public ICommand GetCommand(string key)
    {
        return commands.ContainsKey(key) ? commands[key] : null;
    }
}
```

---

### 3. メニューの設定をJSONで管理 (`menu.json`)

```json
[
    {
        "Name": "ファイル",
        "SubItems": [
            {
                "Name": "開く",
                "Command": "OpenFile"
            },
            {
                "Name": "終了",
                "Command": "Exit"
            }
        ]
    }
]
```

---

### 4. 汎用メニューバークラス (`CustomMenuStrip.cs`)
このクラスでメニューバーを構築し、設定を読み込んでコマンドと関連付けます。

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

public class CustomMenuStrip : MenuStrip
{
    private CommandRegistry commandRegistry;

    public CustomMenuStrip(CommandRegistry registry, string configPath)
    {
        this.commandRegistry = registry;
        LoadMenu(configPath);
    }

    private void LoadMenu(string configPath)
    {
        if (!File.Exists(configPath))
        {
            MessageBox.Show("メニュー設定ファイルが見つかりません");
            return;
        }

        string json = File.ReadAllText(configPath);
        var menuItems = JsonSerializer.Deserialize<List<MenuItemConfig>>(json);

        foreach (var item in menuItems)
        {
            this.Items.Add(CreateMenuItem(item));
        }
    }

    private ToolStripMenuItem CreateMenuItem(MenuItemConfig config)
    {
        ToolStripMenuItem menuItem = new ToolStripMenuItem(config.Name);

        if (!string.IsNullOrEmpty(config.Command))
        {
            ICommand command = commandRegistry.GetCommand(config.Command);
            if (command != null)
            {
                menuItem.Click += (sender, e) => command.Execute();
            }
        }

        foreach (var subItem in config.SubItems)
        {
            menuItem.DropDownItems.Add(CreateMenuItem(subItem));
        }

        return menuItem;
    }
}
```

---

### 5. フォームでの利用 (`MainForm.cs`)

```csharp
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        InitializeMenu();
    }

    private void InitializeMenu()
    {
        CommandRegistry registry = new CommandRegistry();
        registry.Register("OpenFile", new OpenFileCommand());
        registry.Register("Exit", new ExitCommand());

        CustomMenuStrip menuStrip = new CustomMenuStrip(registry, "menu.json");
        this.MainMenuStrip = menuStrip;
        this.Controls.Add(menuStrip);
    }
}
```

---

## 拡張案
1. **動的ロード機能**
   - JSONを変更すれば即座にメニューを変更できる機能を追加。

2. **プラグインシステム**
   - メニューのコマンドを外部のDLLから動的にロード可能にする。

3. **ロール管理**
   - ユーザーの権限に応じて、表示するメニューを制御。

---

このアーキテクチャにより、メニューバーの構成を外部から設定でき、コードの変更なしでカスタマイズが可能になります！